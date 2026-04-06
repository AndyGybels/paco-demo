using LanguageExt.Common;
using MediatR;
using PacoDemo.Api.Application.Queries.AskQuestion;

namespace PacoDemo.Api.Application.Queries.GetConversation;

public record GetConversationQuery(Guid DocumentId) : IRequest<Result<IReadOnlyList<ConversationEntry>>>;

public record ConversationEntry(Guid Id, string Question, string Answer, IReadOnlyList<QuestionSource> Sources, DateTime AskedAt);
