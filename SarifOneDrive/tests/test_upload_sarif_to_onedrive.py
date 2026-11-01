import importlib.util
import json
import tempfile
import unittest
from io import BytesIO
from pathlib import Path
from zipfile import ZipFile


SCRIPT_PATH = (
    Path(__file__).resolve().parents[1]
    / "scripts"
    / "upload_sarif_to_onedrive.py"
)
MODULE_SPEC = importlib.util.spec_from_file_location(
    "upload_sarif_to_onedrive", SCRIPT_PATH
)
uploader = importlib.util.module_from_spec(MODULE_SPEC)
MODULE_SPEC.loader.exec_module(uploader)


class SarifUploadTests(unittest.TestCase):
    def test_reads_sarif_bytes_without_modifying_them(self):
        sarif_bytes = b'\xef\xbb\xbf{"version":"2.1.0","runs":[]}\r\n'
        with tempfile.TemporaryDirectory() as temporary_directory:
            sarif_path = Path(temporary_directory) / "analysis.sarif"
            sarif_path.write_bytes(sarif_bytes)

            self.assertEqual(sarif_bytes, uploader.read_sarif(str(sarif_path)))

    def test_rejects_unsupported_sarif_version(self):
        with tempfile.TemporaryDirectory() as temporary_directory:
            sarif_path = Path(temporary_directory) / "invalid.sarif"
            sarif_path.write_text(
                json.dumps({"version": "2.0.0", "runs": []}),
                encoding="utf-8",
            )

            with self.assertRaisesRegex(ValueError, "supported SARIF version"):
                uploader.read_sarif(str(sarif_path))

    def test_accepts_sarif_1_0_0(self):
        sarif_bytes = b'{"version":"1.0.0","runs":[]}'
        with tempfile.TemporaryDirectory() as temporary_directory:
            sarif_path = Path(temporary_directory) / "analysis.sarif"
            sarif_path.write_bytes(sarif_bytes)

            self.assertEqual(sarif_bytes, uploader.read_sarif(str(sarif_path)))

    def test_archive_contains_only_original_sarif_bytes(self):
        sarif_bytes = b'{"version":"1.0.0","runs":[]}\r\n'
        archive_bytes = uploader.make_archive(
            "owner/repository",
            "2026-11-01",
            sarif_bytes,
        )

        with ZipFile(BytesIO(archive_bytes), "r") as archive:
            self.assertEqual(["owner-repository_2026-11-01.sarif"], archive.namelist())
            self.assertEqual(sarif_bytes, archive.read(archive.namelist()[0]))

    def test_filename_uses_repository_date_and_zip_extension(self):
        self.assertEqual(
            "owner-repo-Duck_2026-11-01.zip",
            uploader.make_filename("owner/repo-Duck", "2026-11-01"),
        )


if __name__ == "__main__":
    unittest.main()
