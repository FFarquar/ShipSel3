using Microsoft.JSInterop;
using ShipSel3.Models;
using ShipSel3.Shared;
namespace ShipSel3.Services.UploadDownloadService
{
    public interface IUploadDownloadServiceClient
    {

        Task<ServiceResponse<Stream>> GetFileAsStream(UnitForGameSystemDTO unit, int rulesetId);

        //Task<ServiceResponse<List<bool>>> DeleteFilesFromFileSystem(List<FileDetail> filesToDelete);

        Task<ServiceResponse<List<UploadResult>>> UploadFiles(List<FileUploadDTO> e, int rulesetId, int countryId);

    }
}
