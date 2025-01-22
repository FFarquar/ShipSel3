using ShipSel3.Models;
using Blazored.LocalStorage;

namespace ShipSel3.Services.LocalStorageService
{
    public class StorageService : IStorageService
    {
        private readonly ILocalStorageService _localStorage;

        public StorageService(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }


        public async Task<SH.ServiceResponse<int>> AddUnitsToStorage(List<SH.UnitForGameSystemDTO> units)
        {
            var exsitItems = await _localStorage.GetItemAsync<List<SH.UnitForGameSystemDTO>>("units");

            SH.ServiceResponse<bool> LocStorageCleared = new SH.ServiceResponse<bool>();
            if (exsitItems != null)
            {
                //have to remove the items from the storage service
                LocStorageCleared = await RemoveAllUnitsFromStorage();
            }

            if (LocStorageCleared.Success == true)
            {
                //Only saving the units selected by the units
                List<SH.UnitForGameSystemDTO> newUnits = new List<SH.UnitForGameSystemDTO>();

                newUnits = units
                    .FindAll(x => x.NumberSelected > 0)
                    .ToList();

                await _localStorage.SetItemAsync("units", newUnits);

                var countOfUnitsInStorage = await GetCountOfUnitsStored();

                if (countOfUnitsInStorage.Data > 0)
                {
                    return new SH.ServiceResponse<int>
                    {
                        Data = countOfUnitsInStorage.Data,
                    };
                }
                else
                {
                    return new SH.ServiceResponse<int>
                    {
                        Data = 0,
                        Message = "Items not added to storage",
                        Success = false
                    };
                }

            }
            else
            {
                //storage wasnt deleted. Dont write the values
                return new SH.ServiceResponse<int>
                {
                    Data = 0,
                    Success = false,
                    Message = "Couldnt remove existing units stored in storage"
                };
            }

        }

        public async Task<SH.ServiceResponse<bool>> RemoveAllUnitsFromStorage()
        {
            var units = await _localStorage.GetItemAsync<List<SH.UnitForGameSystemDTO>>("units");

            if (units == null)
            {
                return new SH.ServiceResponse<bool>
                {
                    Data = true,
                };
            }
            else
            {
                await _localStorage.ClearAsync();

                //Checking to make sure the clear worked
                var unitsLeft = await _localStorage.GetItemAsync<List<SH.UnitForGameSystemDTO>>("units");
                if (unitsLeft == null)
                {
                    return new SH.ServiceResponse<bool>
                    {
                        Data = true,
                    };
                }
                else
                {
                    return new SH.ServiceResponse<bool>
                    {
                        Data = false,
                        Success = false,
                        Message = "Couldnt clear the storage"

                    };

                }

            }
        }

        public async Task<SH.ServiceResponse<int>> GetCountOfUnitsStored()
        {
            var units = await _localStorage.GetItemAsync<List<SH.UnitForGameSystemDTO>>("units");
            if (units != null)
            {
                return new SH.ServiceResponse<int> { Data = units.Count };
            }
            else
            {
                return new SH.ServiceResponse<int>
                {
                    Data = 0,
                    Message = "Nothing found",
                    Success = false
                };
            }
        }

        public async Task<SH.ServiceResponse<List<SH.UnitForGameSystemDTO>>> RetrieveAllUnits()
        {
            var units = await _localStorage.GetItemAsync<List<SH.UnitForGameSystemDTO>>("units");
            if (units == null)
            {
                return new SH.ServiceResponse<List<SH.UnitForGameSystemDTO>>
                {
                    Data = new List<SH.UnitForGameSystemDTO>(),
                    Success = false,
                    Message = "No units found"
                };
            }
            else
            {
                return new SH.ServiceResponse<List<SH.UnitForGameSystemDTO>>
                {
                    Data = units
                };

            }

        }

    }
}
