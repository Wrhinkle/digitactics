"""The audit must never turn missing, stale, or purely numerical evidence into visual approval."""
import importlib.util
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

    def tearDown(self):
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
                   "Sources": [{"Index": 0, "Source": "creature", "Signature": "context", "Representative": 0},
                               {"Index": 1, "Source": "otherContext", "Signature": "context", "Representative": 0}]})
        self.write(self.args.visual / "0001.json", {"Index": 1, "Source": "otherContext", "Asset": "asset", "AliasOf": "creature",
                   "RepresentativeIndex": 0, "Signature": "context", "Image": "0000.png", "Status": "alias-unreviewed", "RuntimeHashes": {"models.json": self.hash}})
        row = self.other_row()
        self.assertEqual(row["renderedTechnicalStatus"], "VERIFIED")
        self.assertEqual(row["evidence"]["renderReusedFrom"], "creature")

    def test_explicit_alias_with_different_context_is_rejected(self):
        self.two_sources()
        self.write(self.args.visual / "identity-index.json", {"CatalogSha256": self.hash, "SourceCount": 2, "IdentityCount": 2,
                   "Sources": [{"Index": 0, "Source": "creature", "Signature": "normal", "Representative": 0},
                               {"Index": 1, "Source": "otherContext", "Signature": "companion", "Representative": 1}]})
        self.write(self.args.visual / "0001.json", {"Index": 1, "Source": "otherContext", "Asset": "asset", "AliasOf": "creature",
                   "RepresentativeIndex": 0, "Signature": "normal", "Image": "0000.png", "Status": "alias-unreviewed", "RuntimeHashes": {"models.json": self.hash}})
        self.assertEqual(self.other_row()["renderedTechnicalStatus"], "UNVERIFIED")

    def test_separate_manual_review_scopes_preserve_unverified(self):
        evidence = audit.ROOT / "review.txt"
        evidence.write_text("Manual review.", encoding="utf-8")
        self.write(self.args.visual / "reviews.json", {"sources": [{"sourceKey": "creature", "visualFidelityStatus": "UNVERIFIED",
                   "motionReviewStatus": "VERIFIED", "fineTextureStatus": "UNVERIFIED", "runtimeHashes": {"models.json": self.hash}, "evidence": ["review.txt"]}]})
        row = self.result()
        self.assertEqual(row["motionReviewStatus"], "VERIFIED")
        self.assertEqual(row["fineTextureStatus"], "UNVERIFIED")
        self.assertEqual(row["visualFidelityStatus"], "UNVERIFIED")


if __name__ == "__main__":
    unittest.main()
