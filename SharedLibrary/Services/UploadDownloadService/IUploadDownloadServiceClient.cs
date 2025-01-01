
using ShMod = SharedLibrary.Models;
namespace SharedLibrary.Services.UploadDownloadService
{
    public interface IUploadDownloadServiceClient
    {

        Task<ShMod.ServiceResponse<Stream>> GetFileAsStream(ShMod.UnitForGameSystemDTO unit, int rulesetId);

        //Task<ServiceResponse<List<bool>>> DeleteFilesFromFileSystem(List<FileDetail> filesToDelete);

        Task<ShMod.ServiceResponse<List<SharedLibrary.Shared.UploadResult>>> UploadFiles(List<SharedLibrary.Shared.FileUploadDTO> e, int rulesetId, int countryId);

    }
}
