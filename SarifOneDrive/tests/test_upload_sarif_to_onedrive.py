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

            with self.assertRaisesRegex(ValueError, "SARIF version 2.1.0"):
                uploader.read_sarif(str(sarif_path))

    def test_rejects_sarif_1_0_0(self):
        sarif_bytes = b'{"version":"1.0.0","runs":[]}'
        with tempfile.TemporaryDirectory() as temporary_directory:
            sarif_path = Path(temporary_directory) / "analysis.sarif"
            sarif_path.write_bytes(sarif_bytes)

            with self.assertRaisesRegex(ValueError, "SARIF version 2.1.0"):
                uploader.read_sarif(str(sarif_path))

    def test_archive_restores_timestamped_project_structure(self):
        sarif_bytes = b'{"version":"2.1.0","runs":[]}\r\n'
        archive_bytes = uploader.make_archive(
            "DuckChat_20261102-004500",
            "20261102-004500",
            sarif_bytes,
        )

        with ZipFile(BytesIO(archive_bytes), "r") as archive:
            expected_path = (
                "DuckChat_20261102-004500/"
                "DuckChat_sarif_20261102-004500.sarif"
            )
            self.assertEqual([expected_path], archive.namelist())
            self.assertEqual(sarif_bytes, archive.read(expected_path))

    def test_filename_uses_project_name_and_timestamp(self):
        self.assertEqual(
            "DuckChat_sarif_20261102-004500.zip",
            uploader.make_filename("DuckChat", "20261102-004500"),
        )


if __name__ == "__main__":
    unittest.main()
