using ShipSel3.Models;


namespace ShipSel3.Services.LocalStorageService
{
    public interface IStorageService
    {
        Task<SH.ServiceResponse<int>> AddUnitsToStorage(List<SH.UnitForGameSystemDTO> units);
        Task<SH.ServiceResponse<bool>> RemoveAllUnitsFromStorage();
        Task<SH.ServiceResponse<List<SH.UnitForGameSystemDTO>>> RetrieveAllUnits();
    }
}
