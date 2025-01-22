using Microsoft.JSInterop;
using ShipSel3.Models;
using ShipSel3.Shared;
namespace ShipSel3.Services.UploadDownloadService
{
    public interface IUploadDownloadServiceClient
    {

        Task<SH.ServiceResponse<Stream>> GetFileAsStream(SH.UnitForGameSystemDTO unit, int rulesetId);

        //Task<ServiceResponse<List<bool>>> DeleteFilesFromFileSystem(List<FileDetail> filesToDelete);

        Task<SH.ServiceResponse<List<UploadResult>>> UploadFiles(List<FileUploadDTO> e, int rulesetId, int countryId);

    }
}
