using LanguageExt.Common;
using MediatR;

namespace PacoDemo.Api.Application.Queries.AskQuestion;

public record AskQuestionQuery(Guid DocumentId, string Question) : IRequest<Result<QuestionResult>>;

public record QuestionResult(string Answer, IReadOnlyList<QuestionSource> Sources);

public record QuestionSource(int PageNumber, int LineNumber, string Excerpt, int ChunkIndex);
