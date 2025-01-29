using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShMod = SharedLibrary.Models;
namespace SharedLibrary.LibraryServices.libraryLocalStorageService
{
    public interface ILocalStorageFromLibrary
    {
        Task<ShMod.ServiceResponse<int>> AddUnitsToStorage(List<ShMod.UnitForGameSystemDTO> units, bool isRestartingGame);
        Task<ShMod.ServiceResponse<bool>> RemoveAllUnitsFromStorage();
        Task<ShMod.ServiceResponse<List<ShMod.UnitForGameSystemDTO>>> RetrieveAllUnits();

        Task<ShMod.ServiceResponse<bool>> IsGameInProgress(string gameIdToCheck);
        Task<ShMod.ServiceResponse<bool>> SaveGameProgressID(string gameId);
        Task<ShMod.ServiceResponse<bool>> SaveAllBroadSDS(List<ShMod.BroadsideSDS> broadsideSDS);
        Task<ShMod.ServiceResponse<bool>> UpdateIndividualSDS(ShMod.BroadsideSDS broadsideSDS);

        Task<ShMod.ServiceResponse<List<ShMod.BroadsideSDS>>> LoadExisitngSDSdetails();
    }
}
