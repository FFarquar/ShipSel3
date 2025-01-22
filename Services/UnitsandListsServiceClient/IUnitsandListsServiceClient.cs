//using ShipSel3.Models;
using ShipSel3.Shared;

namespace ShipSel3.Services.UnitsandListsServiceClient
{
    public interface IUnitsandListsServiceClient
    {
        Task<SH.ServiceResponse<List<SH.RuleSet>>> GetRuleSets();
        Task<SH.ServiceResponse<SH.RuleSet>> GetRuleSet(int rulesetId);
        Task<SH.ServiceResponse<List<SH.Country>>> GetListOfCountries();
        Task<SH.ServiceResponse<List<SH.Country>>> GetListOfCountriesForSelectedUnitsInGameSystem(int gameSystem);
        Task<SH.ServiceResponse<List<SH.UnitType>>> GetListOfUnitTypes();

        Task<SH.ServiceResponse<List<SH.SubUnitTypeDTO>>> GetListOfSubUnits();
        Task<SH.ServiceResponse<int>> AddUnit(SH.Unit unitToAdd);



        Task<SH.ServiceResponse<SH.Unit>> GetUnitWithoutChildObjects(int unitId);
        Task<SH.ServiceResponse<List<SH.GameSystemUnitSpecificDetail>>> GetGameSystemUnitSpecificDetails(int unitId);
        Task<SH.ServiceResponse<List<SH.UnitForGameSystemDTO>>> GetListofAllGameUnitsForRuleset(int rulesetId, bool onlyReturnUnitsInCollection);
        Task SetListOfUnits(int rulesetId, bool onlyReturnUnitsInCollection);

        List<SH.UnitForGameSystemDTO> UnitList { get; set; }
        Task<SH.ServiceResponse<List<SH.UnitWithGameSystemDetails>>> GetListofAllGameUnitsWithGameSpecDetails();
        Task<SH.ServiceResponse<int>> UpdateUnit(SH.Unit unit);
        Task<SH.ServiceResponse<bool>> DeleteUnit(int unitId);
        Task<SH.ServiceResponse<List<SH.OrderCard>>> GetBroadSideOrderCards();
        Task<SH.ServiceResponse<List<SH.DamageCardData>>> GetBroadSideDamageCards();


        Task<SH.ServiceResponse<int>> AddGameSystemUnitSpecificDetail(SH.GameSystemUnitSpecificDetail gamespefic, List<FileUploadDTO> filesToUploadDTO, int countryId);
    }
}
