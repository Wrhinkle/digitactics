"""The audit must never turn missing, stale, or purely numerical evidence into visual approval."""
import importlib.util
from contextlib import chdir
import json
from pathlib import Path
from tempfile import TemporaryDirectory
from types import SimpleNamespace
import unittest

spec = importlib.util.spec_from_file_location("audit_report", Path(__file__).with_name("model-audit-report.py"))
audit = importlib.util.module_from_spec(spec)
spec.loader.exec_module(audit)


class EvidenceChecks(unittest.TestCase):
    def setUp(self):
        self.temp = TemporaryDirectory()
        audit.ROOT = Path(self.temp.name)
        audit.NATIVE = audit.ROOT / "native"
        audit.ARTIFACTS = audit.NATIVE / "artifacts"
        (audit.NATIVE / "content").mkdir(parents=True)
        audit.ARTIFACTS.mkdir()
        self.catalog = {"assets": {"asset": {"sha256": "asset-hash"}}, "sources": {
            "creature": {"asset": "asset", "motion": {"sha256": "motion-hash", "file": "motion.gz"}, "metadata": {"height": 1}}}}
        self.write(audit.NATIVE / "content/models.json", self.catalog)
        self.hash = audit.sha(audit.NATIVE / "content/models.json")
        self.inventory = {"catalogSha256": self.hash, "assets": [{"asset": "asset", "status": "VERIFIED", "failures": [], "review": []}],
                          "sources": [{"sourceKey": "creature", "asset": "asset", "structureStatus": "VERIFIED", "aliases": [],
                                       "companionContexts": [], "roles": [], "clips": [], "failures": [], "review": []}]}
        self.args = SimpleNamespace(groups_only=False, inventory=audit.ARTIFACTS / "inventory.json",
                                    runtime=audit.ARTIFACTS / "runtime.json", visual=audit.ARTIFACTS / "visual",
                                    output=audit.ARTIFACTS / "matrix.json")
        self.args.visual.mkdir()
        self.write(self.args.inventory, self.inventory)
        self.original_source_paths = audit.capture_source_paths if hasattr(audit, "capture_source_paths") else None
        audit.capture_source_paths = lambda: {group: ["native/src/" + group + ".cs"]
                                              for group in ["renderer", "pose", "combat", "companion"]}
        self.original_material_paths = getattr(audit, "material_source_paths", None)
        audit.material_source_paths = lambda: ["native/src/material-loader.cs"]

    def tearDown(self):
        if self.original_source_paths is not None:
            audit.capture_source_paths = self.original_source_paths
        if self.original_material_paths is not None:
            audit.material_source_paths = self.original_material_paths
        self.temp.cleanup()

    @staticmethod
    def write(path, value):
        path.write_text(json.dumps(value), encoding="utf-8")

    def result(self):
        audit.merge(self.args)
        return audit.read(self.args.output)["sources"][0]

    def test_missing_evidence_is_unverified(self):
        self.assertEqual(self.result()["status"], "UNVERIFIED")

    def test_current_finite_sampling_cannot_prove_appearance(self):
        self.write(self.args.runtime, {"catalogSha256": self.hash, "sources": [{"sourceKey": "creature", "runtimeSampling": "FINITE", "status": "UNVERIFIED"}]})
        row = self.result()
        self.assertEqual(row["finiteSamplingStatus"], "VERIFIED")
        self.assertEqual(row["status"], "UNVERIFIED")

    def test_stale_inventory_rejected(self):
        self.inventory["catalogSha256"] = "stale"
        self.write(self.args.inventory, self.inventory)
        with self.assertRaisesRegex(ValueError, "different model catalog"):
            self.result()

    def test_stale_runtime_cannot_pass(self):
        self.write(self.args.runtime, {"catalogSha256": "stale", "sources": [{"sourceKey": "creature", "runtimeSampling": "FINITE", "status": "VERIFIED"}]})
        row = self.result()
        self.assertEqual(row["runtimeStatus"], "UNVERIFIED")
        self.assertEqual(row["finiteSamplingStatus"], "UNVERIFIED")

    def test_changed_inventory_code_rejected(self):
        self.inventory["codeIdentity"] = {"src/changed.cs": "old-hash"}
        self.write(self.args.inventory, self.inventory)
        with self.assertRaisesRegex(ValueError, "source code changed"):
            self.result()

    def test_incomplete_inventory_rejected(self):
        self.inventory["sources"] = []
        self.write(self.args.inventory, self.inventory)
        with self.assertRaisesRegex(ValueError, "every shipped source"):
            self.result()

    def test_real_failure_is_retained(self):
        self.inventory["sources"][0]["structureStatus"] = "FAIL"
        self.write(self.args.inventory, self.inventory)
        self.assertEqual(self.result()["status"], "FAIL")

    def test_manual_pass_without_evidence_is_not_accepted(self):
        self.write(self.args.visual / "reviews.json", {"sources": [{"sourceKey": "creature", "visualFidelityStatus": "VERIFIED", "evidence": []}]})
        self.assertEqual(self.result()["visualFidelityStatus"], "UNVERIFIED")

    def test_identical_assets_with_different_metadata_do_not_share_visual_review(self):
        self.catalog["sources"]["differentScale"] = {"asset": "asset", "motion": {"sha256": "motion-hash"}, "metadata": {"height": 2}}
        self.assertEqual(len(audit.visual_groups(self.catalog)["groups"]), 2)

    def test_stale_manual_failure_is_retained_as_history_not_current_failure(self):
        evidence = audit.ROOT / "old-review.txt"
        evidence.write_text("Historical failed render.", encoding="utf-8")
        self.write(self.args.visual / "reviews.json", {"sources": [{"sourceKey": "creature", "visualFidelityStatus": "FAIL",
                   "runtimeHashes": {"models.json": "old-catalog"}, "evidence": ["old-review.txt"]}]})
        row = self.result()
        self.assertEqual(row["status"], "UNVERIFIED")
        self.assertEqual(row["visualReview"]["visualFidelityStatus"], "FAIL")

    def two_sources(self):
        self.catalog["sources"]["otherContext"] = self.catalog["sources"]["creature"].copy()
        self.write(audit.NATIVE / "content/models.json", self.catalog)
        self.hash = audit.sha(audit.NATIVE / "content/models.json")
        self.inventory["catalogSha256"] = self.hash
        self.inventory["sources"].append({**self.inventory["sources"][0], "sourceKey": "otherContext"})
        self.write(self.args.inventory, self.inventory)
        (self.args.visual / "0000.png").write_bytes(b"fixture-image")
        self.write(self.args.visual / "0000.json", {"Index": 0, "Source": "creature", "Asset": "asset", "Motion": "motion-hash",
                   "Image": "0000.png", "RuntimeHashes": {"models.json": self.hash}, "Samples": [{"Draws": 1, "VisiblePixels": 100}]})
        self.capture()

    def other_row(self):
        audit.merge(self.args)
        return next(row for row in audit.read(self.args.output)["sources"] if row["sourceKey"] == "otherContext")

    def test_same_asset_cannot_implicitly_reuse_another_companion_context(self):
        self.two_sources()
        row = self.other_row()
        self.assertEqual(row["renderedTechnicalStatus"], "UNVERIFIED")
        self.assertIsNone(row["evidence"]["renderReusedFrom"])

    def test_explicit_current_renderer_alias_reuses_exact_context(self):
        self.two_sources()
        self.write(self.args.visual / "identity-index.json", {"CatalogSha256": self.hash, "SourceCount": 2, "IdentityCount": 1,
                   "CompanionCatalogSha256": self.capture_hashes["companion-models.json"],
                   "Sources": [{"Index": 0, "Source": "creature", "Signature": "context", "Representative": 0},
                               {"Index": 1, "Source": "otherContext", "Signature": "context", "Representative": 0}]})
        self.write(self.args.visual / "0001.json", {"Index": 1, "Source": "otherContext", "Asset": "asset", "AliasOf": "creature",
                   "RepresentativeIndex": 0, "Signature": "context", "Image": "0000.png", "Status": "alias-unreviewed", "RuntimeHashes": self.capture_hashes})
        row = self.other_row()
        self.assertEqual(row["renderedTechnicalStatus"], "VERIFIED")
        self.assertEqual(row["evidence"]["renderReusedFrom"], "creature")

    def test_explicit_alias_with_different_context_is_rejected(self):
        self.two_sources()
        self.write(self.args.visual / "identity-index.json", {"CatalogSha256": self.hash, "SourceCount": 2, "IdentityCount": 2,
                   "CompanionCatalogSha256": self.capture_hashes["companion-models.json"],
                   "Sources": [{"Index": 0, "Source": "creature", "Signature": "normal", "Representative": 0},
                               {"Index": 1, "Source": "otherContext", "Signature": "companion", "Representative": 1}]})
        self.write(self.args.visual / "0001.json", {"Index": 1, "Source": "otherContext", "Asset": "asset", "AliasOf": "creature",
                   "RepresentativeIndex": 0, "Signature": "normal", "Image": "0000.png", "Status": "alias-unreviewed", "RuntimeHashes": self.capture_hashes})
        self.assertEqual(self.other_row()["renderedTechnicalStatus"], "UNVERIFIED")

    def test_separate_manual_review_scopes_preserve_unverified(self):
        self.capture()
        evidence = audit.ROOT / "review.txt"
        evidence.write_text("Manual review.", encoding="utf-8")
        self.write(self.args.visual / "reviews.json", {"sources": [{"sourceKey": "creature", "visualFidelityStatus": "UNVERIFIED",
                   "motionReviewStatus": "VERIFIED", "fineTextureStatus": "UNVERIFIED", "runtimeHashes": self.capture_hashes,
                   "reviewScope": {"roles": ["idle", "movement", "attack", "special", "win"], "framesPerRole": 2, "animationFramesInspected": 10},
                   "evidence": ["review.txt", str((self.args.visual / "0000.png").relative_to(audit.ROOT))]}]})
        row = self.result()
        self.assertEqual(row["motionReviewStatus"], "VERIFIED")
        self.assertEqual(row["fineTextureStatus"], "UNVERIFIED")
        self.assertEqual(row["visualFidelityStatus"], "UNVERIFIED")

    def capture(self, companion=False):
        """Create measured capture inputs, not merely matching catalog labels."""
        companions = {"bases": {}, "forms": {}, "fallbacks": {}}
        if companion:
            companions["bases"]["pet"] = {"source": "creature", "height": 1, "unit": False,
                                           "clips": {"idle": "idle", "walk": "walk"}, "procedural": {}}
        self.write(audit.NATIVE / "content/companion-models.json", companions)
        frozen = {}
        for name, value in [("models.json", self.catalog), ("companion-models.json", companions)]:
            path = self.args.visual / ("frozen-" + name)
            self.write(path, value)
            frozen[name] = {"file": path.name, "sha256": audit.sha(path)}
        groups = {}
        for group in ["renderer", "pose", "combat", "companion"]:
            path = audit.NATIVE / ("src/" + group + ".cs")
            path.parent.mkdir(exist_ok=True)
            path.write_text("measured " + group, encoding="utf-8")
            groups[group] = {"native/src/" + group + ".cs": audit.sha(path)}
        self.capture_hashes = {name: row["sha256"] for name, row in frozen.items()}
        self.capture_hashes.update({name: name + "-hash" for name in ["DigiTactics.Desktop.dll", "DigiTactics.Assets.dll",
                                                                   "DigiTactics.Presentation.dll", "DigiTactics.Rules.dll"]})
        self.write(self.args.visual / "capture-manifest.json", {"schemaVersion": 1,
                   "policy": "ModelAnimator+CombatChoreography+CompanionAnimator/v2", "frozenCatalogs": frozen,
                   "runtimeHashes": self.capture_hashes, "sourceGroups": groups, "sourceStableBeforeAfterBuild": True})
        roles = ["idle", "movement", "attack", "special", "win"] + (["companion-walk"] if companion else [])
        for name in ["0000.png", "0000.texture.png"]:
            (self.args.visual / name).write_bytes(b"measured-image")
        self.write(self.args.visual / "0000.json", {"Index": 0, "Source": "creature", "Asset": "asset",
                   "Motion": "motion-hash", "Image": "0000.png", "TextureImage": "0000.texture.png",
                   "RuntimeHashes": self.capture_hashes, "Frames": 2, "Roles": roles,
                   "Samples": [{"Role": role, "Frame": frame, "Draws": 1, "VisiblePixels": 100}
                               for role in roles for frame in range(2)]})
        self.write(self.args.visual / "identity-index.json", {"CatalogSha256": self.capture_hashes["models.json"],
                   "CompanionCatalogSha256": self.capture_hashes["companion-models.json"],
                   "Sources": [{"Index": i, "Source": key, "Signature": "context", "Representative": 0}
                               for i, key in enumerate(self.catalog["sources"])]})

    def update_catalog(self):
        self.write(audit.NATIVE / "content/models.json", self.catalog)
        self.inventory["catalogSha256"] = audit.sha(audit.NATIVE / "content/models.json")
        self.write(self.args.inventory, self.inventory)

    def test_unchanged_source_survives_unrelated_catalog_change(self):
        self.capture()
        self.catalog["unrelatedExportMetadata"] = "new"
        self.update_catalog()
        self.assertEqual(self.result()["renderedTechnicalStatus"], "VERIFIED")

    def test_changed_asset_payload_cannot_reuse_capture(self):
        self.capture()
        self.catalog["assets"]["asset"]["sha256"] = "changed-payload"
        self.update_catalog()
        self.assertEqual(self.result()["renderedTechnicalStatus"], "UNVERIFIED")

    def test_changed_metadata_cannot_reuse_capture(self):
        self.capture()
        self.catalog["sources"]["creature"]["metadata"]["height"] = 2
        self.update_catalog()
        self.assertEqual(self.result()["renderedTechnicalStatus"], "UNVERIFIED")

    def test_changed_companion_context_invalidates_capture(self):
        self.capture(companion=True)
        path = audit.NATIVE / "content/companion-models.json"
        companions = audit.read(path)
        companions["bases"]["pet"]["height"] = 3
        self.write(path, companions)
        self.assertEqual(self.result()["renderedTechnicalStatus"], "UNVERIFIED")

    def test_changed_renderer_source_invalidates_capture(self):
        self.capture()
        (audit.NATIVE / "src/renderer.cs").write_text("changed rendering", encoding="utf-8")
        self.assertEqual(self.result()["renderedTechnicalStatus"], "UNVERIFIED")

    def test_companion_only_runtime_change_preserves_combat_only_source(self):
        self.capture()
        (audit.NATIVE / "src/companion.cs").write_text("new gait", encoding="utf-8")
        self.assertEqual(self.result()["renderedTechnicalStatus"], "VERIFIED")

    def test_companion_runtime_change_invalidates_companion_source(self):
        self.capture(companion=True)
        (audit.NATIVE / "src/companion.cs").write_text("new gait", encoding="utf-8")
        self.assertEqual(self.result()["renderedTechnicalStatus"], "UNVERIFIED")

    def test_unmeasured_manifest_cannot_prove_current_render(self):
        self.capture()
        path = self.args.visual / "capture-manifest.json"
        manifest = audit.read(path)
        manifest["sourceStableBeforeAfterBuild"] = False
        self.write(path, manifest)
        self.assertEqual(self.result()["renderedTechnicalStatus"], "UNVERIFIED")

    def test_mismatched_render_binary_rejected(self):
        self.capture()
        path = self.args.visual / "0000.json"
        record = audit.read(path)
        record["RuntimeHashes"]["DigiTactics.Assets.dll"] = "another-binary"
        self.write(path, record)
        self.assertEqual(self.result()["renderedTechnicalStatus"], "UNVERIFIED")

    def test_partial_role_samples_cannot_complete_render_coverage(self):
        self.capture()
        path = self.args.visual / "0000.json"
        record = audit.read(path)
        record["Samples"].pop()
        self.write(path, record)
        self.assertEqual(self.result()["renderedTechnicalStatus"], "UNVERIFIED")

    def test_frozen_catalog_tampering_rejected(self):
        self.capture()
        (self.args.visual / "frozen-models.json").write_text("{}", encoding="utf-8")
        self.assertEqual(self.result()["renderedTechnicalStatus"], "UNVERIFIED")

    def manual_review(self):
        record = audit.read(self.args.visual / "0000.json")
        paths = [str((self.args.visual / name).relative_to(audit.ROOT)) for name in ["0000.json", "0000.png", "0000.texture.png"]]
        review = {"sourceKey": "creature", "visualFidelityStatus": "VERIFIED", "motionReviewStatus": "VERIFIED",
                  "fineTextureStatus": "VERIFIED", "runtimeHashes": self.capture_hashes, "evidence": paths,
                  "evidenceSha256": {path: audit.sha(audit.ROOT / path) for path in paths},
                  "reviewScope": {"roles": record["Roles"], "framesPerRole": 2,
                                  "animationFramesInspected": len(record["Samples"]), "textureViewsInspected": 2}}
        self.write(self.args.visual / "reviews.json", {"sources": [review]})
        self.write(self.args.runtime, {"catalogSha256": audit.sha(audit.NATIVE / "content/models.json"),
                   "sources": [{"sourceKey": "creature", "runtimeSampling": "FINITE", "status": "VERIFIED"}]})
        return review

    def test_complete_measured_and_reviewed_visual_scopes_can_pass(self):
        self.capture()
        self.manual_review()
        self.assertEqual(self.result()["visualFidelityStatus"], "VERIFIED")

    def test_partial_manual_roles_cannot_approve_overall(self):
        self.capture()
        review = self.manual_review()
        review["reviewScope"]["roles"] = ["idle"]
        self.write(self.args.visual / "reviews.json", {"sources": [review]})
        row = self.result()
        self.assertEqual(row["motionReviewStatus"], "UNVERIFIED")
        self.assertEqual(row["status"], "UNVERIFIED")

    def test_missing_texture_review_cannot_approve_overall(self):
        self.capture()
        review = self.manual_review()
        review["reviewScope"]["textureViewsInspected"] = 0
        self.write(self.args.visual / "reviews.json", {"sources": [review]})
        row = self.result()
        self.assertEqual(row["fineTextureStatus"], "UNVERIFIED")
        self.assertEqual(row["status"], "UNVERIFIED")

    def test_changed_reviewed_image_cannot_inherit_manual_approval(self):
        self.capture()
        self.manual_review()
        (self.args.visual / "0000.png").write_bytes(b"replacement-image")
        self.assertEqual(self.result()["visualFidelityStatus"], "UNVERIFIED")

    def test_motion_review_requires_actual_animation_evidence(self):
        self.capture()
        review = self.manual_review()
        review["evidence"] = [path for path in review["evidence"] if not path.endswith("0000.png")]
        self.write(self.args.visual / "reviews.json", {"sources": [review]})
        self.assertEqual(self.result()["motionReviewStatus"], "UNVERIFIED")

    def test_source_without_companion_survives_another_source_change(self):
        self.two_sources()
        self.catalog["sources"]["otherContext"]["metadata"] = {"height": 3}
        self.update_catalog()
        self.assertEqual(self.result()["renderedTechnicalStatus"], "VERIFIED")

    def test_texture_review_survives_companion_only_runtime_change(self):
        self.capture(companion=True)
        self.manual_review()
        (audit.NATIVE / "src/companion.cs").write_text("new gait", encoding="utf-8")
        row = self.result()
        self.assertEqual(row["fineTextureStatus"], "VERIFIED")
        self.assertEqual(row["motionReviewStatus"], "UNVERIFIED")
        self.assertEqual(row["status"], "UNVERIFIED")

    def test_current_render_error_without_loaded_motion_is_failure(self):
        self.capture()
        self.write(self.args.visual / "0000.json", {"Index": 0, "Source": "creature", "Asset": "asset",
                   "RuntimeHashes": self.capture_hashes, "Status": "render-error", "Error": "Model failed to load."})
        self.assertEqual(self.result()["renderedTechnicalStatus"], "FAIL")

    def test_stale_runtime_error_remains_history(self):
        self.write(self.args.runtime, {"catalogSha256": "stale", "sources": [{"sourceKey": "creature", "Error": "old error"}]})
        self.assertEqual(self.result()["runtimeStatus"], "UNVERIFIED")

    def test_relative_cli_visual_directory_accepts_manifest(self):
        self.capture()
        self.args.visual = self.args.visual.relative_to(audit.ROOT)
        with chdir(audit.ROOT):
            self.assertEqual(self.result()["renderedTechnicalStatus"], "VERIFIED")

    def test_new_combat_dependency_keeps_unchanged_texture_review(self):
        self.capture()
        self.manual_review()
        (audit.NATIVE / "src/new-combat.cs").write_text("new combat wrapper", encoding="utf-8")
        original = audit.capture_source_paths
        audit.capture_source_paths = lambda: {**original(), "combat": ["native/src/combat.cs", "native/src/new-combat.cs"]}
        row = self.result()
        self.assertEqual(row["fineTextureStatus"], "VERIFIED")
        self.assertEqual(row["motionReviewStatus"], "UNVERIFIED")
        self.assertEqual(row["status"], "UNVERIFIED")

    def test_changed_combat_only_code_preserves_texture_review(self):
        self.capture()
        self.manual_review()
        (audit.NATIVE / "src/combat.cs").write_text("new movement", encoding="utf-8")
        row = self.result()
        self.assertEqual(row["fineTextureStatus"], "VERIFIED")
        self.assertEqual(row["motionReviewStatus"], "UNVERIFIED")

    def test_changed_pose_code_invalidates_texture_review(self):
        self.capture()
        self.manual_review()
        (audit.NATIVE / "src/pose.cs").write_text("new pose evaluator", encoding="utf-8")
        self.assertEqual(self.result()["fineTextureStatus"], "UNVERIFIED")

    def test_legacy_pose_hash_can_be_derived_from_exact_combat_entry(self):
        self.capture()
        self.manual_review()
        path = self.args.visual / "capture-manifest.json"
        manifest = audit.read(path)
        manifest["sourceGroups"]["combat"].update(manifest["sourceGroups"].pop("pose"))
        self.write(path, manifest)
        self.assertEqual(self.result()["fineTextureStatus"], "VERIFIED")

    def test_missing_pose_hash_cannot_approve_texture(self):
        self.capture()
        self.manual_review()
        path = self.args.visual / "capture-manifest.json"
        manifest = audit.read(path)
        del manifest["sourceGroups"]["pose"]
        self.write(path, manifest)
        self.assertEqual(self.result()["fineTextureStatus"], "UNVERIFIED")

    def identity_review(self):
        proof = audit.ROOT / "identity-proof.txt"
        proof.write_text("Manually compared declared form with exact asset and authoritative reference.", encoding="utf-8")
        self.args.identity = audit.NATIVE / "content/model-identity-reviews.json"
        record = {"sourceKey": "creature", "status": "FAIL", "asset": "asset", "assetSha256": "asset-hash",
                  "sourceSha256": None, "declaredForm": None, "review": "Wrong species assigned.",
                  "evidence": ["identity-proof.txt"], "evidenceSha256": {"identity-proof.txt": audit.sha(proof)}}
        self.write(self.args.identity, {"schemaVersion": 1, "sources": [record]})
        return record

    def test_exact_asset_identity_failure_survives_missing_render_and_runtime(self):
        self.identity_review()
        row = self.result()
        self.assertEqual(row["identityStatus"], "FAIL")
        self.assertEqual(row["functionalStatus"], "FAIL")
        self.assertEqual(row["status"], "FAIL")

    def test_identity_failure_survives_animation_and_renderer_changes(self):
        self.capture()
        self.identity_review()
        self.catalog["sources"]["creature"]["motion"]["sha256"] = "fixed-animation"
        self.catalog["sources"]["creature"]["metadata"]["height"] = 2
        self.update_catalog()
        (audit.NATIVE / "src/renderer.cs").write_text("new renderer")
        self.assertEqual(self.result()["identityStatus"], "FAIL")

    def test_replaced_asset_invalidates_old_identity_failure(self):
        self.identity_review()
        self.catalog["assets"]["asset"]["sha256"] = "replacement"
        self.update_catalog()
        row = self.result()
        self.assertEqual(row["identityStatus"], "UNVERIFIED")
        self.assertEqual(row["identityReview"]["status"], "FAIL")

    def test_changed_declared_form_invalidates_old_identity_failure(self):
        self.identity_review()
        self.catalog["sources"]["creature"]["metadata"]["form"] = "ActualForm"
        self.update_catalog()
        self.assertEqual(self.result()["identityStatus"], "UNVERIFIED")

    def test_remapped_asset_does_not_inherit_identity_failure(self):
        self.identity_review()
        self.catalog["assets"]["another-asset"] = self.catalog["assets"]["asset"].copy()
        self.catalog["sources"]["creature"]["asset"] = "another-asset"
        self.inventory["assets"].append({**self.inventory["assets"][0], "asset": "another-asset"})
        self.inventory["sources"][0]["asset"] = "another-asset"
        self.update_catalog()
        self.assertEqual(self.result()["identityStatus"], "UNVERIFIED")

    def test_changed_original_source_hash_invalidates_identity_record(self):
        self.identity_review()
        self.catalog["assets"]["asset"]["sourceSha256"] = "different-import"
        self.update_catalog()
        self.assertEqual(self.result()["identityStatus"], "UNVERIFIED")

    def test_identity_failure_requires_unchanged_review_proof(self):
        self.identity_review()
        (audit.ROOT / "identity-proof.txt").write_text("different finding")
        self.assertEqual(self.result()["identityStatus"], "UNVERIFIED")

    def test_identity_approval_alone_never_proves_functional_quality(self):
        review = self.identity_review()
        review["status"] = "VERIFIED"
        self.write(self.args.identity, {"schemaVersion": 1, "sources": [review]})
        row = self.result()
        self.assertEqual(row["identityStatus"], "VERIFIED")
        self.assertEqual(row["functionalStatus"], "UNVERIFIED")

    def test_duplicate_identity_decisions_are_rejected(self):
        review = self.identity_review()
        self.write(self.args.identity, {"schemaVersion": 1, "sources": [review, review]})
        with self.assertRaisesRegex(ValueError, "Duplicate identity"):
            self.result()

    def material_review(self):
        proof = audit.ROOT / "material-proof.txt"
        proof.write_text("Reviewed diffuse slot uses a gray specular gradient on visible armor.", encoding="utf-8")
        loader = audit.ROOT / audit.material_source_paths()[0]
        loader.parent.mkdir(parents=True, exist_ok=True)
        loader.write_text("load original material binding", encoding="utf-8")
        self.args.materials = audit.NATIVE / "content/model-material-reviews.json"
        record = {"issueId": "armor-gradient", "asset": "asset", "assetSha256": "asset-hash", "sourceSha256": "original-hash",
                  "status": "FAIL", "review": "Confirmed material-slot defect, not a heuristic candidate.",
                  "codeHashes": {str(loader.relative_to(audit.ROOT)).replace("\\", "/"): audit.sha(loader)},
                  "evidence": ["material-proof.txt"], "evidenceSha256": {"material-proof.txt": audit.sha(proof)}}
        self.catalog["assets"]["asset"]["sourceSha256"] = "original-hash"
        self.update_catalog()
        self.write(self.args.materials, {"schemaVersion": 1, "assets": [record]})
        return record

    def test_confirmed_asset_material_failure_needs_no_fresh_motion_capture(self):
        self.material_review()
        row = self.result()
        self.assertEqual(row["sourceMaterialStatus"], "FAIL")
        self.assertEqual(row["materialRuntimeStatus"], "FAIL")
        self.assertEqual(row["functionalStatus"], "FAIL")

    def test_material_failure_survives_unrelated_motion_change(self):
        self.material_review()
        self.catalog["sources"]["creature"]["motion"]["sha256"] = "new-motion"
        self.update_catalog()
        self.assertEqual(self.result()["functionalStatus"], "FAIL")

    def test_material_defect_applies_to_every_exact_asset_alias(self):
        self.two_sources()
        self.material_review()
        self.assertEqual(self.other_row()["materialRuntimeStatus"], "FAIL")

    def test_material_replacement_retains_old_finding_as_history(self):
        self.material_review()
        self.catalog["assets"]["asset"].update(sha256="repaired-payload", sourceSha256="repaired-source")
        self.update_catalog()
        row = self.result()
        self.assertEqual(row["sourceMaterialStatus"], "UNVERIFIED")
        self.assertEqual(row["functionalStatus"], "UNVERIFIED")
        self.assertEqual(row["sourceMaterialReviews"][0]["status"], "FAIL")

    def test_original_material_source_change_alone_invalidates_finding(self):
        self.material_review()
        self.catalog["assets"]["asset"]["sourceSha256"] = "changed-original"
        self.update_catalog()
        self.assertEqual(self.result()["sourceMaterialStatus"], "UNVERIFIED")

    def test_material_loader_change_requires_new_runtime_review_but_keeps_asset_fact(self):
        self.material_review()
        (audit.ROOT / audit.material_source_paths()[0]).write_text("new corrected loader")
        row = self.result()
        self.assertEqual(row["sourceMaterialStatus"], "FAIL")
        self.assertEqual(row["materialRuntimeStatus"], "UNVERIFIED")
        self.assertEqual(row["functionalStatus"], "UNVERIFIED")

    def test_material_finding_missing_loader_dependency_cannot_fail_runtime(self):
        record = self.material_review()
        record["codeHashes"] = {}
        self.write(self.args.materials, {"schemaVersion": 1, "assets": [record]})
        row = self.result()
        self.assertEqual(row["sourceMaterialStatus"], "FAIL")
        self.assertEqual(row["materialRuntimeStatus"], "UNVERIFIED")

    def test_changed_material_proof_invalidates_canonical_finding(self):
        self.material_review()
        (audit.ROOT / "material-proof.txt").write_text("different finding")
        self.assertEqual(self.result()["sourceMaterialStatus"], "UNVERIFIED")

    def test_material_candidate_is_not_a_confirmed_defect(self):
        record = self.material_review()
        record["status"] = "CANDIDATE"
        self.write(self.args.materials, {"schemaVersion": 1, "assets": [record]})
        self.assertEqual(self.result()["sourceMaterialStatus"], "UNVERIFIED")

    def test_duplicate_material_issue_rejected(self):
        record = self.material_review()
        self.write(self.args.materials, {"schemaVersion": 1, "assets": [record, record]})
        with self.assertRaisesRegex(ValueError, "Duplicate material"):
            self.result()

    def functional_review(self):
        self.capture()
        review = self.manual_review()
        review["visualFidelityStatus"] = "UNVERIFIED"
        review["functionalStatus"] = "VERIFIED"
        review["functionalScope"] = {"identityReviewed": True, "materialMappingReviewed": True,
            "transitionsReviewed": True, "runtimeBoundsReviewed": True, "unresolvedFindings": [],
            "boardContextRequired": False, "boardContextReviewed": False}
        self.write(self.args.visual / "reviews.json", {"sources": [review]})
        code = {}
        for name in audit.runtime_source_paths():
            path = audit.NATIVE / name
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_text("runtime measured " + name)
            code[name] = audit.sha(path)
        runtime = {"catalogSha256": self.hash, "codeHashes": code, "sources": [{"sourceKey": "creature",
            "runtimeSampling": "FINITE", "status": "UNVERIFIED", "review": [], "phases": [
                {"name": name, "sampleCount": 33} for name in ["idle-cycle", "attack-cycle", "special-cycle", "win-cycle",
                    "attack-runtime", "special-runtime", "win-runtime", "attack-to-special", "special-to-win",
                    "movement-Grounded", "movement-arrival"]]}]}
        self.write(self.args.runtime, runtime)
        return review, runtime

    def test_functional_quality_can_pass_without_browser_fidelity_approval(self):
        self.functional_review()
        row = self.result()
        self.assertEqual(row["visualFidelityStatus"], "UNVERIFIED")
        self.assertEqual(row["functionalStatus"], "VERIFIED")
        self.assertEqual(row["identityStatus"], "VERIFIED")
        self.assertEqual(row["status"], "VERIFIED")

    def test_functional_approval_requires_current_transition_measurement(self):
        review, runtime = self.functional_review()
        runtime["sources"][0]["phases"] = [p for p in runtime["sources"][0]["phases"] if p["name"] != "attack-to-special"]
        self.write(self.args.runtime, runtime)
        self.assertEqual(self.result()["functionalStatus"], "UNVERIFIED")

    def test_functional_approval_rejects_incomplete_runtime_code_identity(self):
        review, runtime = self.functional_review()
        runtime["codeHashes"].pop(next(iter(runtime["codeHashes"])))
        self.write(self.args.runtime, runtime)
        self.assertEqual(self.result()["functionalStatus"], "UNVERIFIED")

    def test_functional_approval_requires_explicit_identity_review(self):
        review, runtime = self.functional_review()
        review["functionalScope"]["identityReviewed"] = False
        self.write(self.args.visual / "reviews.json", {"sources": [review]})
        self.assertEqual(self.result()["functionalStatus"], "UNVERIFIED")

    def test_functional_approval_does_not_hide_an_open_finding(self):
        review, runtime = self.functional_review()
        review["functionalScope"]["unresolvedFindings"] = ["Feet do not step during movement."]
        self.write(self.args.visual / "reviews.json", {"sources": [review]})
        self.assertEqual(self.result()["functionalStatus"], "UNVERIFIED")

    def test_functional_approval_cannot_override_known_wrong_species(self):
        self.functional_review()
        self.identity_review()
        self.assertEqual(self.result()["functionalStatus"], "FAIL")

    def test_clipping_candidate_needs_actual_board_review(self):
        review, runtime = self.functional_review()
        runtime["sources"][0]["review"] = ["special: large root travel"]
        self.write(self.args.runtime, runtime)
        self.assertEqual(self.result()["functionalStatus"], "UNVERIFIED")

    def test_board_review_with_hashed_evidence_can_resolve_framing_candidate(self):
        review, runtime = self.functional_review()
        runtime["sources"][0]["review"] = ["special: large root travel"]
        self.write(self.args.runtime, runtime)
        board = audit.ROOT / "board.png"
        board.write_bytes(b"reviewed production context")
        review["evidence"].append("board.png")
        review["evidenceSha256"]["board.png"] = audit.sha(board)
        review["functionalScope"].update(boardContextRequired=True, boardContextReviewed=True,
                                         boardSamplesInspected=3, boardEvidence=["board.png"])
        self.write(self.args.visual / "reviews.json", {"sources": [review]})
        self.assertEqual(self.result()["functionalStatus"], "VERIFIED")

    def test_optional_fidelity_difference_does_not_override_functional_acceptance(self):
        review, runtime = self.functional_review()
        review["visualFidelityStatus"] = "FAIL"
        self.write(self.args.visual / "reviews.json", {"sources": [review]})
        row = self.result()
        self.assertEqual(row["visualFidelityStatus"], "FAIL")
        self.assertEqual(row["functionalStatus"], "VERIFIED")

    def test_fidelity_approval_without_functional_review_does_not_silently_promote(self):
        self.capture()
        self.manual_review()
        self.assertEqual(self.result()["functionalStatus"], "UNVERIFIED")

    def test_partial_current_review_can_record_concrete_functional_failure(self):
        self.capture()
        review = self.manual_review()
        review.update(functionalStatus="FAIL", motionReviewStatus="UNVERIFIED", fineTextureStatus="UNVERIFIED")
        review["reviewScope"] = {"roles": ["attack"], "framesPerRole": 1, "animationFramesInspected": 1}
        self.write(self.args.visual / "reviews.json", {"sources": [review]})
        self.assertEqual(self.result()["functionalStatus"], "FAIL")

    def test_stale_partial_functional_failure_remains_history(self):
        self.capture()
        review = self.manual_review()
        review.update(functionalStatus="FAIL", motionReviewStatus="UNVERIFIED", fineTextureStatus="UNVERIFIED")
        self.write(self.args.visual / "reviews.json", {"sources": [review]})
        (audit.NATIVE / "src/combat.cs").write_text("fixed production movement")
        row = self.result()
        self.assertEqual(row["functionalStatus"], "UNVERIFIED")
        self.assertEqual(row["visualReview"]["functionalStatus"], "FAIL")


if __name__ == "__main__":
    unittest.main()
