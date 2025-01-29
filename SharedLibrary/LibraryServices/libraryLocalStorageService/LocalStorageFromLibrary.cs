using Blazored.LocalStorage;
using SharedLibrary.Models;
using SharedLibrary.LibraryServices.libraryLocalStorageService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShMod = SharedLibrary.Models;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;

namespace SharedLibrary.LibraryServices.libraryLocalStorageService
{
    public class LocalStorageFromLibrary : ILocalStorageFromLibrary
    {

        private readonly ILocalStorageService _localStorage;

        public LocalStorageFromLibrary(ILocalStorageService localStorage)

        {
            _localStorage = localStorage;
        }

        //public async Task<ShMod.ServiceResponse<int>> AddUnitsToStorage(List<ShMod.UnitForGameSystemDTO> units)
        //{

        //    var exsitItems = await _localStorage.GetItemAsync<List<ShMod.UnitForGameSystemDTO>>("units");

        //    ShMod.ServiceResponse<bool> LocStorageCleared = new ShMod.ServiceResponse<bool>();
        //    if (exsitItems != null)
        //    {
        //        //have to remove the items from the storage service
        //        LocStorageCleared = await RemoveAllUnitsFromStorage();
        //    }

        //    if (LocStorageCleared.Success == true)
        //    {
        //        //Only saving the units selected by the units
        //        List<ShMod.UnitForGameSystemDTO> newUnits = new List<ShMod.UnitForGameSystemDTO>();

        //        newUnits = units
        //            .FindAll(x => x.NumberSelected > 0)
        //            .ToList();

        //        await _localStorage.SetItemAsync("units", newUnits);

        //        var countOfUnitsInStorage = await GetCountOfUnitsStored();

        //        if (countOfUnitsInStorage.Data > 0)
        //        {
        //            return new ShMod.ServiceResponse<int>
        //            {
        //                Data = countOfUnitsInStorage.Data,
        //            };
        //        }
        //        else
        //        {
        //            return new ShMod.ServiceResponse<int>
        //            {
        //                Data = 0,
        //                Message = "Items not added to storage",
        //                Success = false
        //            };
        //        }

        //    }
        //    else
        //    {
        //        //storage wasnt deleted. Dont write the values
        //        return new ShMod.ServiceResponse<int>
        //        {
        //            Data = 0,
        //            Success = false,
        //            Message = "Couldnt remove existing units stored in storage"
        //        };
        //    }

        //}

        public async Task<ShMod.ServiceResponse<int>> AddUnitsToStorage(List<ShMod.UnitForGameSystemDTO> units, bool isRestartingGame)
        {

            var exsitItems = await _localStorage.GetItemAsync<List<ShMod.UnitForGameSystemDTO>>("units");

            //System.Diagnostics.Debugger.Break();
            if (!isRestartingGame)
            {
                //if game is being started from new
                ShMod.ServiceResponse<bool> LocStorageCleared = new ShMod.ServiceResponse<bool>();
                if (exsitItems != null)
                {
                    //have to remove the items from the storage service
                    LocStorageCleared = await RemoveAllUnitsFromStorage();
                }

                if (LocStorageCleared.Success == false)
                {

                    //storage wasnt deleted. Dont write the values
                    return new ShMod.ServiceResponse<int>
                    {
                        Data = 0,
                        Success = false,
                        Message = "Couldnt remove existing units stored in storage"
                    };
                }

            }
            else {
                //if game is being restarted

                //delete the existing units .. then add them again. Dont delete anything else

                await _localStorage.RemoveItemAsync("units");
            }

            //Only saving the units selected by the units
            List<ShMod.UnitForGameSystemDTO> newUnits = new List<ShMod.UnitForGameSystemDTO>();

            newUnits = units
                .FindAll(x => x.NumberSelected > 0)
                .ToList();

            await _localStorage.SetItemAsync("units", newUnits);

            var countOfUnitsInStorage = await GetCountOfUnitsStored();

            if (countOfUnitsInStorage.Data > 0)
            {
                return new ShMod.ServiceResponse<int>
                {
                    Data = countOfUnitsInStorage.Data,
                };
            }
            else
            {
                return new ShMod.ServiceResponse<int>
                {
                    Data = 0,
                    Message = "Items not added to storage",
                    Success = false
                };
            }


        }

        public async Task<ShMod.ServiceResponse<bool>> RemoveAllUnitsFromStorage()
        {
            var units = await _localStorage.GetItemAsync<List<ShMod.UnitForGameSystemDTO>>("units");

            if (units == null)
            {
                return new ShMod.ServiceResponse<bool>
                {
                    Data = true,
                };
            }
            else
            {

                await _localStorage.ClearAsync();
                //await _localStorage.RemoveItemAsync("units");

                //Checking to make sure the clear worked
                var unitsLeft = await _localStorage.GetItemAsync<List<ShMod.UnitForGameSystemDTO>>("units");
                if (unitsLeft == null)
                {
                    return new ShMod.ServiceResponse<bool>
                    {
                        Data = true,
                    };
                }
                else
                {
                    return new ShMod.ServiceResponse<bool>
                    {
                        Data = false,
                        Success = false,
                        Message = "Couldnt clear the storage"
                    };
                }
            }
        }

        public async Task<ShMod.ServiceResponse<int>> GetCountOfUnitsStored()
        {
            var units = await _localStorage.GetItemAsync<List<ShMod.UnitForGameSystemDTO>>("units");
            if (units != null)
            {
                return new ShMod.ServiceResponse<int> { Data = units.Count };
            }
            else
            {
                return new ShMod.ServiceResponse<int>
                {
                    Data = 0,
                    Message = "Nothing found",
                    Success = false
                };
            }
        }

        public async Task<ShMod.ServiceResponse<List<ShMod.UnitForGameSystemDTO>>> RetrieveAllUnits()
        {
            var units = await _localStorage.GetItemAsync<List<ShMod.UnitForGameSystemDTO>>("units");
            if (units == null)
            {
                return new ShMod.ServiceResponse<List<ShMod.UnitForGameSystemDTO>>
                {
                    Data = new List<ShMod.UnitForGameSystemDTO>(),
                    Success = false,
                    Message = "No units found"
                };
            }
            else
            {
                return new ShMod.ServiceResponse<List<ShMod.UnitForGameSystemDTO>>
                {
                    Data = units
                };

            }
        }

        public async Task<ShMod.ServiceResponse<bool>> IsGameInProgress(string gameIdToCheck)
        {
            string gameIdFromStorage = await _localStorage.GetItemAsync<string>("gameIdinProgress");

            //default response
            ShMod.ServiceResponse<bool> vsrReturn = new ShMod.ServiceResponse<bool>
            {
                Data = false,
                Success = false,
                Message = "No game found"
            };


            if (gameIdFromStorage != null)
            {

                if (gameIdFromStorage == gameIdToCheck)
                {
                    //found the game
                    vsrReturn.Success = true;
                    vsrReturn.Data = true;
                }

            }

            return vsrReturn;
        }

        public async Task<ServiceResponse<bool>> SaveGameProgressID(string gameId)
        {

            try
            {

                await _localStorage.SetItemAsync<string>("gameIdinProgress", gameId);



                return new ShMod.ServiceResponse<bool>
                {
                    Data = true,
                    Success = true,
                    Message = "Saved"
                };
            }
            catch (Exception)
            {
                return new ShMod.ServiceResponse<bool>
                {
                    Data = false,
                    Success = false,
                    Message = "Not saved"
                };

            }

        }

        public async Task<ShMod.ServiceResponse<List<ShMod.BroadsideSDS>>> LoadExisitngSDSdetails()
        {
            //default response
            ShMod.ServiceResponse<List<ShMod.BroadsideSDS>> vsrReturn = new ShMod.ServiceResponse<List<ShMod.BroadsideSDS>>
            {
                Data = null,
                Success = false,
                Message = "Coudlnt read existing details"
            };

            try 
            {
                var exsitItems = await _localStorage.GetItemAsync<List<ShMod.BroadsideSDS>>("broadSideSDS");

                if (exsitItems != null)
                {
                    vsrReturn.Data = exsitItems;
                    vsrReturn.Success = true;
                    vsrReturn.Message = "SDS info recalled";
                }
            }
            catch (Exception)
            {
                //just return the default service response if there is an issue

            }
            return vsrReturn;
        }

        public async Task<ServiceResponse<bool>> SaveAllBroadSDS(List<ShMod.BroadsideSDS> broadsideSDSList)
        {

            //default response
            ShMod.ServiceResponse<bool> vsrReturn = new ShMod.ServiceResponse<bool>
            {
                Data = false,
                Success = false,
                Message = "Coudlnt write broadside SDS to local storage"
            };

            var exsitItems = await _localStorage.GetItemAsync<List<ShMod.BroadsideSDS>>("broadSideSDS");

            ShMod.ServiceResponse<bool> LocStorageCleared = new ShMod.ServiceResponse<bool>();

            if (exsitItems != null)
            {
                //have to remove any existring broadside SDS from the storage service
                await _localStorage.RemoveItemAsync("broadSideSDS");

                vsrReturn.Data = true;
                vsrReturn.Success = true;
                vsrReturn.Message = "SDS info saved";
            }

            try
            {
                await _localStorage.SetItemAsync("broadSideSDS", broadsideSDSList);
            }
            catch (Exception)
            {

                throw;
            }

            return vsrReturn;
        }

        public async Task<ServiceResponse<bool>> UpdateIndividualSDS(ShMod.BroadsideSDS broadsideSDS)
        {

            //get a list of the saved items
            ShMod.ServiceResponse<bool> vsrReturn = new ShMod.ServiceResponse<bool>
            {
                Data = false,
                Success = false,
                Message = "Coudlnt update individual file"
            };
            

            try
            {
                //System.Diagnostics.Debugger.Break();
                var units = await _localStorage.GetItemAsync<List<ShMod.BroadsideSDS>>("broadSideSDS");

                if (units != null)
                {

                    int indexToReplace = -1;
                    foreach (var unit in units)
                    {
                        if (unit.id == broadsideSDS.id)
                        {
                            indexToReplace = units.IndexOf(unit);

                            //break; //this is causing the flow of program to jump out of method, not just the foreach

                        }
                    }

                    if (indexToReplace >-1)
                        {
                            //replace the item in the collection with the revised item
                            units[indexToReplace] = broadsideSDS;

                            //remove all the saved details for the SDS details
                            await _localStorage.RemoveItemAsync("broadSideSDS");

                            //save the list again
                            await _localStorage.SetItemAsync("broadSideSDS", units);

                            vsrReturn.Success = true;
                            vsrReturn.Data = true;
                            vsrReturn.Message = "Found the item";
                        }
                }
            }
            catch (Exception)
            {

                throw;
            }
            






            return vsrReturn;
        }

    }
}






