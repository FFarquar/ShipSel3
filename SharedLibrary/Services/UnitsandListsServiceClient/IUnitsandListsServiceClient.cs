
using SharedLibrary.Shared;
using ShMod = SharedLibrary.Models;
namespace SharedLibrary.Services.UnitsandListsServiceClient
{
    public interface IUnitsandListsServiceClient
    {
        Task<ShMod.ServiceResponse<List<ShMod.RuleSet>>> GetRuleSets();
        Task<ShMod.ServiceResponse<ShMod.RuleSet>> GetRuleSet(int rulesetId);
        Task<ShMod.ServiceResponse<List<ShMod.Country>>> GetListOfCountries();
        Task<ShMod.ServiceResponse<List<ShMod.Country>>> GetListOfCountriesForSelectedUnitsInGameSystem(int gameSystem);
        Task<ShMod.ServiceResponse<List<ShMod.UnitType>>> GetListOfUnitTypes();

        Task<ShMod.ServiceResponse<List<ShMod.SubUnitTypeDTO>>> GetListOfSubUnits();
        Task<ShMod.ServiceResponse<int>> AddUnit(ShMod.Unit unitToAdd);

        
        
        Task<ShMod.ServiceResponse<ShMod.Unit>> GetUnitWithoutChildObjects(int unitId);
        Task<ShMod.ServiceResponse<List<ShMod.GameSystemUnitSpecificDetail>>> GetGameSystemUnitSpecificDetails(int unitId);
        Task<ShMod.ServiceResponse<List<ShMod.UnitForGameSystemDTO>>> GetListofAllGameUnitsForRuleset(int rulesetId, bool onlyReturnUnitsInCollection);
        Task SetListOfUnits(int rulesetId, bool onlyReturnUnitsInCollection);

        List<ShMod.UnitForGameSystemDTO> UnitList {get;set;}
        Task<ShMod.ServiceResponse<List<ShMod.UnitWithGameSystemDetails>>> GetListofAllGameUnitsWithGameSpecDetails();
        Task<ShMod.ServiceResponse<int>> UpdateUnit(ShMod.Unit unit);
        Task<ShMod.ServiceResponse<bool>> DeleteUnit(int unitId);
        Task<ShMod.ServiceResponse<List<ShMod.OrderCard>>> GetBroadSideOrderCards();
        Task<ShMod.ServiceResponse<List<ShMod.DamageCardData>>> GetBroadSideDamageCards();


        Task<ShMod.ServiceResponse<int>> AddGameSystemUnitSpecificDetail(ShMod.GameSystemUnitSpecificDetail gamespefic, List<FileUploadDTO> filesToUploadDTO, int countryId);
        Task<ShMod.ServiceResponse<int>> UpdateGameSystemUnitSpecificDetail(ShMod.GameSystemUnitSpecificDetail gamespefic, int countryId);

        Task<ShMod.ServiceResponse<bool>> DeleteGameSystemUnitSpecifiDetails(int gameSystemSpecId);
    }
}
