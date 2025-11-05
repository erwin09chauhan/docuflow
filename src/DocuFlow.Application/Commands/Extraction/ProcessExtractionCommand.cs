using DocuFlow.Application.Common;
using MediatR;

namespace DocuFlow.Application.Commands.Extraction;

public record ProcessExtractionCommand(
    Guid DocumentId
) : IRequest<Result>;