# Bundled location data

## `india-post-pincodes.csv.gz`

The **All India Pincode Directory** from the Department of Posts, published on the Open
Government Data Platform India (data.gov.in) under the
[Government Open Data License – India (GODL-India)](https://data.gov.in/government-open-data-license-india).
GODL allows commercial use with attribution. Show "Contains data from the Department of Posts,
Government of India (data.gov.in)" wherever location data is credited.

- Rows: 157,126 post offices; 19,300 PIN codes; 737 districts; 36 states/UTs
- Columns used: `OfficeName`, `Pincode`, `District`, `StateName`
- Fetched 2026-10-11 from the GitHub mirror `dropdevrahul/pincodes-india` (`pincode.csv`),
  which republishes the data.gov.in file unchanged (data.gov.in's API was unreachable).
  SHA-256 of the uncompressed CSV:
  `84af12fa29adddedfa9addcf46546000d89d2e2899b2993dc88050fafc8861e2`

## Updating

1. Download the latest CSV from data.gov.in ("All India Pincode Directory"). It must have the
   four columns above.
2. Compress it deterministically, so the same input gives the same bytes:
   `gzip -9 -n -c pincode.csv > india-post-pincodes.csv.gz`
3. Run `dotnet test`. The parser tests fail if a new state name doesn't match `RegionSeed`.
4. Deploy. On startup `PostalDirectoryImporter` sees the new content hash and reloads the
   districts and localities in one transaction.

When you change the parsing or normalisation rules but not the file, bump `ParserRevision` in
`BundledPostalDirectory`. The import version is the file hash plus that revision, so databases
that are already loaded pick up the new rules on their next start.
