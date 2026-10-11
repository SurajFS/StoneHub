namespace Identity.Application.Dtos;

// Unmatched profiles keep their free text. The B3 data-quality report lists them for correction.
public sealed record AddressBackfillReportDto(int Matched, int Unmatched);
