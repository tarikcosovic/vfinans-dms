using DMS.Application.UseCases.Auth;
using DMS.Application.UseCases.Documents;
using DMS.Application.UseCases.Games;
using DMS.Application.UseCases.Users;
using Microsoft.Extensions.DependencyInjection;

namespace DMS.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<RegisterUseCase>();
        services.AddScoped<LoginUseCase>();
        services.AddScoped<RequestUploadUrlUseCase>();
        services.AddScoped<ConfirmUploadUseCase>();
        services.AddScoped<ListDocumentsUseCase>();
        services.AddScoped<GetDownloadUrlUseCase>();
        services.AddScoped<GetPreviewUrlUseCase>();
        services.AddScoped<DeleteDocumentUseCase>();
        services.AddScoped<ListGameLeaderboardUseCase>();
        services.AddScoped<SubmitGameScoreUseCase>();
        services.AddScoped<ListClientApprovalsUseCase>();
        services.AddScoped<ListClientCompanyNamesUseCase>();
        services.AddScoped<ApproveClientUseCase>();
        services.AddScoped<DeactivateClientUseCase>();
        services.AddScoped<ChangeOwnPasswordUseCase>();
        services.AddScoped<SetClientPasswordUseCase>();
        return services;
    }
}
