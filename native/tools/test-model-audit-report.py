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

    def tearDown(self):
        if self.original_source_paths is not None:
            audit.capture_source_paths = self.original_source_paths
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

    def test_complete_measured_and_reviewed_scopes_can_pass(self):
        self.capture()
        self.manual_review()
        self.assertEqual(self.result()["status"], "VERIFIED")

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


if __name__ == "__main__":
    unittest.main()
