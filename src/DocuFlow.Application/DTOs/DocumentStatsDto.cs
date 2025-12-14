namespace DocuFlow.Application.DTOs;

public record DocumentStatsDto(
    int Total,
    int Completed,
    int Failed,
    int Processing
);