//using NavalGame.Client.Services.UploadDownloadService;
using System.Net.Http.Json;
using System.Drawing;
using SharedLibrary.Models;
using SharedLibrary.Shared;

using ShMod = SharedLibrary.Models;
using SharedLibrary.Services.UploadDownloadService;
using Newtonsoft.Json;
using System.Text.Json;
using System.Collections.Generic;
using File = System.IO.File;

namespace SharedLibrary.Services.UnitsandListsServiceClient
{
    public class UnitsandListsServiceClient : IUnitsandListsServiceClient
    {
        private readonly HttpClient _http;
        private readonly IUploadDownloadServiceClient _UDSC;

        public UnitsandListsServiceClient(HttpClient http, IUploadDownloadServiceClient Udsc)
        {
            _http = http;
            _UDSC = Udsc;
        }
        public List<ShMod.UnitForGameSystemDTO> UnitList { get; set; } = new List<ShMod.UnitForGameSystemDTO>(); //a list that contains all units. The printpage will have access to this

        private class cutDownUnit
        {
            public int Id { get; set; }
            public int SubUnitTypeId { get; set; }
            public int CountryId { get;set; }
            public int NumberinClass_shipSub { get; set; }
            public string Name_ClassName { get; set; } = string.Empty;
            public string ShipsSubsInClass { get; set; } = string.Empty;
        }

        private class cutDownGameSystemSpecific
        {
            public int Id { get; set; }
            public int UnitId { get; set; }
            public int Cost { get; set; }
            public string ImagePath { get; set; }
            public int RulesetId { get; set; }
        }
        public async Task<ShMod.ServiceResponse<int>> AddUnit(ShMod.Unit unitToAdd)
        {

            //Have to read in the entire unit list and then de-serialize it so I can add the new item to the list and seriailze


            var UnitJS = await _http.GetFromJsonAsync<cutDownUnit[]>("Data/units.json");
            List<cutDownUnit> unitList = UnitJS.ToList();

            var simplteUnit = new cutDownUnit
            {
                Id = unitList.Last().Id+1,
                SubUnitTypeId = unitToAdd.SubUnitTypeId,
                CountryId = unitToAdd.CountryId,
                NumberinClass_shipSub = unitToAdd.NumberinClass_shipSub,
                Name_ClassName = unitToAdd.Name_ClassName,
                ShipsSubsInClass = unitToAdd.ShipsSubsInClass
            };

            unitList.Add(simplteUnit);
            
            string json = System.Text.Json.JsonSerializer.Serialize(unitList, new JsonSerializerOptions { WriteIndented = true });

            try
            {
                await System.IO.File.WriteAllTextAsync("E:\\ProjectsForGit\\ShipSel3\\ShipSel3\\wwwroot\\Data\\units.json", json);

                var sr = new ShMod.ServiceResponse<int>
                {
                    Message = "All good",
                    Success = true
                };
                return sr;

            }
            catch (Exception ex) {
            {
                    var sr = new ShMod.ServiceResponse<int>
                    {
                        Message = "The files weren't uploaded correctly. Image not added " + ex.Message,
                        Success = false
                    };
                    return sr;
                }
            }
            

          //  var result = await _http.PostAsJsonAsync($"data/units.json", jsonObject);

            //return await result.Content.ReadFromJsonAsync<ShMod.ServiceResponse<int>>();
        }

        public async Task<ShMod.ServiceResponse<int>> UpdateGameSystemUnitSpecificDetail(ShMod.GameSystemUnitSpecificDetail gamespefic, int countryId)
        {

            if(gamespefic.ImagePath!= "not changing")
            {
                //if a file is being replaced, the original will need to be deleted

                string basePath = "E:\\ProjectsForGit\\ShipSel3\\ShipSel3\\wwwroot\\ShipImages\\" + gamespefic.RulesetId.ToString() + "\\" + countryId + "\\";

                bool exists = Directory.Exists(basePath);

                if (!exists)
                    Directory.CreateDirectory(basePath);

                try
                {
                    File.Copy(gamespefic.ImagePath, basePath + Path.GetFileName(gamespefic.ImagePath), true);
                }
                catch (Exception)
                {
                    //a test value to return data
                    var srfail1 = new ShMod.ServiceResponse<int>
                    {
                        Message = "Couldnt copy file",
                        Success = true
                    };
                    return srfail1;
                }
            }


            var jsGameSpecList = await _http.GetFromJsonAsync<cutDownGameSystemSpecific[]>("Data/gameSystemSpecific.json");
            List<cutDownGameSystemSpecific> gameSpecList = jsGameSpecList.ToList();

            int index = -1;
            index = gameSpecList.FindIndex(item => item.Id == gamespefic.Id);

            string fileName =  Path.GetFileName(gamespefic.ImagePath);


            if (index >= 0)
            {
                //this means the item exists. Want to replace it 
                cutDownGameSystemSpecific replacemeant = new cutDownGameSystemSpecific();

                replacemeant.UnitId = gamespefic.UnitId;
                replacemeant.Id = gamespefic.Id;
                replacemeant.Cost = gamespefic.Cost;
                replacemeant.ImagePath = fileName;
                replacemeant.RulesetId = gamespefic.RulesetId;

                gameSpecList[index] = replacemeant;
            }
            else
            {
                //add a new item
                cutDownGameSystemSpecific newItem = new cutDownGameSystemSpecific();

                newItem = new cutDownGameSystemSpecific();

                newItem.Id = gameSpecList[gameSpecList.Count - 1].Id + 1;
                newItem.UnitId= gamespefic.UnitId;
                newItem.Cost = gamespefic.Cost;
                newItem.ImagePath = fileName;
                newItem.RulesetId = gamespefic.RulesetId;

                gameSpecList.Add(newItem);

            }


            string json = System.Text.Json.JsonSerializer.Serialize(gameSpecList, new JsonSerializerOptions { WriteIndented = true });

            try
            {
                await System.IO.File.WriteAllTextAsync("E:\\ProjectsForGit\\ShipSel3\\ShipSel3\\wwwroot\\Data\\gameSystemSpecific.json", json);

                var sr = new ShMod.ServiceResponse<int>
                {
                    Message = "Unit saved",
                    Success = true
                };
                return sr;

            }
            catch (Exception ex)
            {
                {
                    var sr = new ShMod.ServiceResponse<int>
                    {
                        Message = "GameSystemSpecific not saved" + ex.Message,
                        Success = false
                    };
                    return sr;
                }
            }

            
        }

        public async Task<ShMod.ServiceResponse<int>> AddGameSystemUnitSpecificDetail(ShMod.GameSystemUnitSpecificDetail gamespefic, List<FileUploadDTO> browserFiles, int countryId)
        {

            // This doesnt work. Replaced with simpler UpdateGameSystemUnitSpecificDetail method that creates and modifies in one function
            string basePath = "E:\\ProjectsForGit\\ShipSel3\\ShipSel3\\wwwroot\\ShipImages\\" + gamespefic.RulesetId.ToString() + "\\" + countryId+"\\";


            //instead of using the upload client, just going to do a copy for the files. Will onlyt be one in this implementation (not multiple files)
            foreach (var file in browserFiles) {
            
                string fileToCopy = file.FileName;
                try
                {

                }
                catch (Exception)
                {

                    throw;
                }
                
            }



            //a test value to return data
            var sr = new ShMod.ServiceResponse<int>
            {
                Message = "Done",
                Success = true
            };
            return sr;

                //var filesUpload = new ShMod.ServiceResponse<List<UploadResult>>();
                //if (browserFiles.Count > 0)
                //{
                //    filesUpload = await _UDSC.UploadFiles(browserFiles, gamespefic.RulesetId, countryId);
                //    if (!filesUpload.Success)
                //    {
                //        //the files weren't uploaded
                //        var sr = new ShMod.ServiceResponse<int>
                //        {
                //            Message = "The files weren't uploaded correctly. Image not added " + filesUpload.Message,
                //            Success = false
                //        };
                //        return sr;
                //    }
                //}

                //var result = await _http.PostAsJsonAsync("api/unitdetails/addgamespecificDetailsForUnit", gamespefic);

                //return await result.Content.ReadFromJsonAsync<ShMod.ServiceResponse<int>>();
         }

        public async Task<ShMod.ServiceResponse<List<ShMod.Country>>> GetListOfCountriesForSelectedUnitsInGameSystem(int ruleSetId)
        {
            //this function will get a list of countries that have actually had units assigned

            ShMod.ServiceResponse<List<ShMod.Country>> fullCountryList = await GetListOfCountries();

            var randomid = Guid.NewGuid().ToString();
            var gameSystemSpecificURL = $"Data/gameSystemSpecific.json?{randomid}";
            var GameSystemUnitJs = await _http.GetFromJsonAsync<ShMod.GameSystemUnitSpecificDetail[]>(gameSystemSpecificURL);
            //var GameSystemUnitJs = await _http.GetFromJsonAsync<GameSystemUnitSpecificDetail[]>("Data/gameSystemSpecific.json");
            var UnitJS = await _http.GetFromJsonAsync<ShMod.Unit[]>("Data/units.json");
            List<ShMod.GameSystemUnitSpecificDetail> gameSpecList = GameSystemUnitJs.ToList();
            List<ShMod.Unit> unitList = UnitJS.ToList();

            List<ShMod.Country> filteredCountryList = new List<ShMod.Country>();

            foreach (var c in fullCountryList.Data)
            {
                var query = unitList
                                .Join(gameSpecList,
                                    ut => ut.Id,
                                    gsu => gsu.UnitId,
                                    (ut, gsu) => new {Unit = ut,  GameSystemUnitSpecificDetail = gsu})
                                .Where(x => x.Unit.CountryId == c.Id && x.GameSystemUnitSpecificDetail.RulesetId == ruleSetId);


                if (query.Count() > 0)
                {
                    filteredCountryList.Add(c);
                }
            }

            return new ShMod.ServiceResponse<List<ShMod.Country>>
            {
                Data = filteredCountryList
            };
        }

        public async Task<ShMod.ServiceResponse<List<ShMod.Country>>> GetListOfCountries()
        {
            var result = await _http.GetFromJsonAsync<ShMod.Country[]>("Data/countries.json");

            if (result != null && result.Length != 0)
            {
                return new ShMod.ServiceResponse<List<ShMod.Country>>
                {
                    Data = result.ToList()
                };
            }
            else
            {
                return new ShMod.ServiceResponse<List<ShMod.Country>>
                {
                    Data = new List<ShMod.Country>(),
                    Success = false,
                    Message = "No countries found"
                };

            }
            //            var result = await _http.GetFromJsonAsync<ShMod.ServiceResponse<List<Country>>>("api/unitdetails/countries");
            //            return result;
        }

        public async Task<ShMod.ServiceResponse<List<ShMod.SubUnitTypeDTO>>> GetListOfSubUnits()
        {

            var rawSubunit = await _http.GetStringAsync("Data/subUnitTypes.json");
            var subUnits = JsonConvert.DeserializeObject<List<ShMod.SubUnitType>>(rawSubunit,

               new JsonSerializerSettings
               {
                   NullValueHandling = NullValueHandling.Ignore,
                   Converters =
                   {
                       new ColorConverter()
                   }
               });

            //Need to add the unit type object for subunit

            var unitTypesJS = await _http.GetFromJsonAsync<ShMod.UnitType[]>("Data/unitTypes.json");

            List<ShMod.UnitType> unitTypeList = unitTypesJS.ToList();

            var _includeUnitType = subUnits
                    .Join(unitTypeList,
                        su => su.UnitTypeId,
                            ut => ut.Id,
                                (su, ut) => new { SubUnitType = su, UnitType = ut });

            //if (subUnits != null && subUnits.Count > 0)
                if (_includeUnitType != null)

                {
                List<ShMod.SubUnitTypeDTO> subunitresponse = new List<ShMod.SubUnitTypeDTO>();
                foreach (var item in _includeUnitType)
                {
                    var subunit = new ShMod.SubUnitTypeDTO
                    {
                        Id = item.SubUnitType.Id,
                        SubUnitName = item.SubUnitType.SubUnitName,
                        UnitTypeName = item.UnitType.Name,
                        UnitTypeId = item.UnitType.Id,
                        RGBDetails = new ShMod.RGBDetails()
                        {
                            R = item.SubUnitType.PrintColour.R,
                            G = item.SubUnitType.PrintColour.G,
                            B = item.SubUnitType.PrintColour.B,
                        }
                    };
                    subunitresponse.Add(subunit);
                }

                return new ShMod.ServiceResponse<List<ShMod.SubUnitTypeDTO>>
                {
                    Data = subunitresponse
                };


            }
            else
            {
                return new ShMod.ServiceResponse<List<ShMod.SubUnitTypeDTO>>
                {
                    Data = new List<ShMod.SubUnitTypeDTO>(),
                    Success = false,
                    Message = "No Sub units found"
                };

            }
            //var result = await _http.GetFromJsonAsync<ShMod.ServiceResponse<List<SubUnitTypeDTO>>>("api/unitdetails/subunits");
            //return result;


        }

        public async Task<ShMod.ServiceResponse<List<ShMod.UnitType>>> GetListOfUnitTypes()
        {

            var result = await _http.GetFromJsonAsync<ShMod.UnitType[]>("Data/unitTypes.json");

            if (result != null && result.Length != 0)
            {
                return new ShMod.ServiceResponse<List<ShMod.UnitType>>
                {
                    Data = result.ToList()
                };
            }
            else
            {
                return new ShMod.ServiceResponse<List<ShMod.UnitType>>
                {
                    Data = new List<ShMod.UnitType>(),
                    Success = false,
                    Message = "No Unit types found"
                };

            }
            //var result = await _http.GetFromJsonAsync<ShMod.ServiceResponse<List<UnitType>>>("api/unitdetails/unittypes");
            //return result;
        }

        public async Task<ShMod.ServiceResponse<List<ShMod.RuleSet>>> GetRuleSets()
        {
            //var result = await _http.GetFromJsonAsync<RuleSet[]>("Data/ruleSets.json");
            //Console.WriteLine("resulet " + result);
            //List<RuleSet> rs = result.ToList();

            //Console.WriteLine("Rule set cout = " + rs.Count);

            var response = await _http.GetAsync(($"Data/ruleSets.json"));
            var jsonResult = await response.Content.ReadFromJsonAsync<List<ShMod.RuleSet>>();
            Console.WriteLine("jsonResult " + jsonResult.ToString());
            List<ShMod.RuleSet> rs = jsonResult.ToList();

            //if (result != null && result.Length != 0)
             if (jsonResult != null && rs.Count != 0)
                {
                return new ShMod.ServiceResponse<List<ShMod.RuleSet>>
                {
                    Data = rs,
                    Message = "all good"
                };
            }
            else
            {
                return new ShMod.ServiceResponse<List<ShMod.RuleSet>>
                {
                    Data = new List<ShMod.RuleSet>(),
                    Success = false,
                    Message = "No rulesets found"
                };

            }

        }

        public async Task<ShMod.ServiceResponse<ShMod.Unit>> GetUnitWithoutChildObjects(int unitId)
        {
            var result = await _http.GetFromJsonAsync<ShMod.ServiceResponse<ShMod.Unit>>("api/unitdetails/getUnitNoChildObjects/" + unitId);
            return result;
        }

        public async Task<ShMod.ServiceResponse<List<ShMod.GameSystemUnitSpecificDetail>>> GetGameSystemUnitSpecificDetails(int unitId)
        {
            //Cant call the function to get a list of rule sets
            List<ShMod.RuleSet> rs = new List<ShMod.RuleSet>();
            var response = await GetRuleSets();
             rs = response.Data;

            //var response = await _http.GetAsync(($"Data/ruleSets.json"));
            //var jsonResult = await response.Content.ReadFromJsonAsync<List<RuleSet>>();
            //Console.WriteLine("jsonResult " + jsonResult.ToString());
            //List<RuleSet> rs = jsonResult.ToList();

            //try
            //{
            //    Console.WriteLine("RS count " + rs.Count.ToString());
            //    Console.WriteLine("Response  = {1} "+ response.ToString());
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine("Exception " + e.Message);
            //    throw;
            //}

            //Console.WriteLine("RS count  before next data read " + rs.Count.ToString());

            var GameSystemUnitJs = await _http.GetFromJsonAsync<ShMod.GameSystemUnitSpecificDetail[]>("Data/gameSystemSpecific.json");
            //Console.WriteLine("RS count  after next data read " + rs.Count.ToString());

            //Console.WriteLine("GameSystems  count = {0} id of first ruleset = {1} rulesetname = {2}", rs.Count().ToString(), rs[1].Id.ToString(), rs[1].RulesetName);

            //get a list of rule systems this unit is a part of

            var query =
                from gss in GameSystemUnitJs
                join rules in rs on gss.RulesetId equals rules.Id
                where gss.UnitId == unitId
                select gss;

            //Console.WriteLine("Query in GetGameSystemUnitSpecificDetails =  " + query.Count());

            List<ShMod.GameSystemUnitSpecificDetail> result = new List<ShMod.GameSystemUnitSpecificDetail>();
            foreach (ShMod.GameSystemUnitSpecificDetail item in query)
            {
                ShMod.GameSystemUnitSpecificDetail gssd = new ShMod.GameSystemUnitSpecificDetail();
                gssd.Cost = item.Cost;
                gssd.Id = item.Id;
                gssd.ImagePath= item.ImagePath;
                //gssd.Ruleset = item.Ruleset;
                gssd.Ruleset = rs.Find(x => x.Id == item.RulesetId);
                gssd.RulesetId = item.RulesetId;
                gssd.Unit = null;
                gssd.UnitId = item.UnitId;
                result.Add(gssd);
            }

            if (result != null && result.Count != 0)
            {

                //Console.WriteLine("Found {0} rule sets for unitId {1} ", result.Count().ToString(), result[0].UnitId.ToString());
                return new ShMod.ServiceResponse<List<ShMod.GameSystemUnitSpecificDetail>>
                {
                    Data = result
                }; 
            }
            else
            {
                return new ShMod.ServiceResponse<List<ShMod.GameSystemUnitSpecificDetail>>
                {
                    Data = new List<ShMod.GameSystemUnitSpecificDetail>(),
                    Success = false,
                    Message = "No gamesec found"
                };

            }
            //var result = await _http.GetFromJsonAsync<ShMod.ServiceResponse<List<GameSystemUnitSpecificDetail>>>("api/unitdetails/getGameSystemUnitSpecificDetails/" + unitId);
            //return result;

        }

        public async Task<ShMod.ServiceResponse<ShMod.RuleSet>> GetRuleSet(int rulesetId)
        {

            var result = await _http.GetFromJsonAsync<ShMod.RuleSet[]>("Data/ruleSets.json");

            if (result != null && result.Length != 0)
            {
                var rs = result
                    .Where(item => item.Id == rulesetId)
                    .FirstOrDefault();
                return new ShMod.ServiceResponse<ShMod.RuleSet>
                {
                    Data = rs,
            };
            }
            else
            {
                return new ShMod.ServiceResponse<ShMod.RuleSet>
                {
                    Data = new ShMod.RuleSet(),
                    Success = false,
                    Message = "No rulesets found"
                };

            }

            //var result = await _http.GetFromJsonAsync<ShMod.ServiceResponse<RuleSet>>("api/UnitDetails/getRuleSet/" + rulesetId);
            //return result;
            //asd
        }

        public async Task<ShMod.ServiceResponse<List<ShMod.UnitForGameSystemDTO>>> GetListofAllGameUnitsForRuleset(int rulesetId, bool onlyReturnUnitsInCollection)
        {
            //var result = await _http.GetFromJsonAsync<ShMod.ServiceResponse<List<UnitForGameSystemDTO>>>("api/unitdetails/allunitsbyGameSystemandType/" + rulesetId + "/" + onlyReturnUnitsInCollection);
            var CountriesJs = await _http.GetFromJsonAsync<ShMod.Country[]>("Data/countries.json");
            var UnitsJs = await _http.GetFromJsonAsync<ShMod.Unit[]>("Data/units.json");

            var rawSubunit = await _http.GetStringAsync("Data/subUnitTypes.json");
            //  var message = JsonConvert.DeserializeObject<List<SubUnitType>>(rawSubunit);

            var SubUnitTypesJs = JsonConvert.DeserializeObject<List<ShMod.SubUnitType>>(rawSubunit,
               new JsonSerializerSettings
               {
                   NullValueHandling = NullValueHandling.Ignore,
                   Converters =
                   {
                       new ColorConverter()
                   }
               });


            var GameSystemUnitJs = await _http.GetFromJsonAsync<ShMod.GameSystemUnitSpecificDetail[]>("Data/gameSystemSpecific.json");

            List<ShMod.GameSystemUnitSpecificDetail> gameSpecList = GameSystemUnitJs.ToList();
            List<ShMod.Country> countryList = CountriesJs.ToList();
            List<ShMod.Unit> unitList = UnitsJs.ToList();
            List<ShMod.SubUnitType> subUnitList = SubUnitTypesJs.ToList();



            //this works
            var _GSWithUnit = gameSpecList
                    .Join(unitList,
                        gsu => gsu.UnitId,
                            u => u.Id,
                            (gsu, u) => new { GameSystemUnitSpecificDetail = gsu, Unit = u })
                    .Where(t => t.GameSystemUnitSpecificDetail.RulesetId == rulesetId);


            //https://asusualcoding.wordpress.com/2018/03/10/join-multiple-lists-using-linq-in-c/
            //This doesnt woek, gets stuck in a loop
            //var _GSWithUnit1 = gameSpecList
            //        .Join(unitList, gsu => gsu.UnitId, u => u.Id, (gsu, u) => new { GameSystemUnitSpecificDetail = gsu, Unit = u })
            //        .Join(countryList, gsu => gsu.Unit.CountryId, country => country.Id, (gsu, country) => new { GameSystemUnitSpecificDetail = gsu, Country = country})
            //        .Where(t => t.GameSystemUnitSpecificDetail.GameSystemUnitSpecificDetail.RulesetId == rulesetId);

            //This adds Subunits to the query
            var _includeSubUnit = _GSWithUnit
                    .Join(subUnitList,
                        gsu => gsu.Unit.SubUnitTypeId,
                            su => su.Id,
                                (gsu, su) => new { GameSystemUnitSpecificDetail = gsu, SubUnitType = su });

            //This works based on the above query
            //foreach (var item in _includeSubUnit)
            //{

            //    Console.WriteLine();
            //    Console.WriteLine("SubUnitName = " + item.SubUnitType.SubUnitName.ToString());
            //    Console.WriteLine("subUnitTypeObj = " + item.SubUnitType.ToString());
            //    Console.WriteLine("Name_ClassName = " + item.GameSystemUnitSpecificDetail.Unit.Name_ClassName.ToString());
            //    Console.WriteLine("Ruleset ID = " + item.GameSystemUnitSpecificDetail.GameSystemUnitSpecificDetail.RulesetId.ToString());
            //    Console.WriteLine("Image path = " + item.GameSystemUnitSpecificDetail.GameSystemUnitSpecificDetail.ImagePath.ToString());
            //}

            //Now need to add countries
            var _includeCountries = _includeSubUnit
                    .Join(countryList,
                        gsu => gsu.GameSystemUnitSpecificDetail.Unit.CountryId,
                            c => c.Id,
                                (gsu, c) => new { GameSystemUnitSpecificDetail = gsu, Country = c });

            

            //foreach (var item in _includeCountries)
            //{

            //    Console.WriteLine();
            //    Console.WriteLine("SubUnitName = " + item.GameSystemUnitSpecificDetail.SubUnitType.SubUnitName.ToString());
            //    Console.WriteLine("subUnitTypeObj = " + item.GameSystemUnitSpecificDetail.SubUnitType.ToString());
            //    Console.WriteLine("Name_ClassName = " + item.GameSystemUnitSpecificDetail.GameSystemUnitSpecificDetail.Unit.Name_ClassName.ToString());
            //    Console.WriteLine("Ruleset ID = " + item.GameSystemUnitSpecificDetail.GameSystemUnitSpecificDetail.GameSystemUnitSpecificDetail.RulesetId.ToString()); ;
            //    Console.WriteLine("Image path = " + item.GameSystemUnitSpecificDetail.GameSystemUnitSpecificDetail.GameSystemUnitSpecificDetail.ImagePath.ToString());
            //    Console.WriteLine("Country name  = " + item.Country.CountryName.ToString());
            //}



            //TODO: Add in subunit type to this, some how or add another query

            List<ShMod.UnitForGameSystemDTO> tempList = new List<ShMod.UnitForGameSystemDTO>();    
            foreach (var item in _includeCountries)
                    {
                        var ShipsSubsInClassCleaned = item.GameSystemUnitSpecificDetail.GameSystemUnitSpecificDetail.Unit.ShipsSubsInClass;
                            if (ShipsSubsInClassCleaned != null)
                            {
                                ShipsSubsInClassCleaned = ShipsSubsInClassCleaned.Trim();
                             }
                ShMod.UnitForGameSystemDTO ug = new ShMod.UnitForGameSystemDTO(){
                            Id = item.GameSystemUnitSpecificDetail.GameSystemUnitSpecificDetail.Unit.Id,
                            Name_ClassName = item.GameSystemUnitSpecificDetail.GameSystemUnitSpecificDetail.Unit.Name_ClassName,
                            subUnitTypeObj = item.GameSystemUnitSpecificDetail.SubUnitType,
                            Alliance = await ReturnAllianceName(item.Country.AllianceId),
                            NumberinClass_shipSub = item.GameSystemUnitSpecificDetail.GameSystemUnitSpecificDetail.Unit.NumberinClass_shipSub,
                            Countryobj = item.Country,
                           Cost = item.GameSystemUnitSpecificDetail.GameSystemUnitSpecificDetail.GameSystemUnitSpecificDetail.Cost,

                            ImagePath = item.GameSystemUnitSpecificDetail.GameSystemUnitSpecificDetail.GameSystemUnitSpecificDetail.ImagePath,
                            ShipsSubsInClass = ShipsSubsInClassCleaned,
                            NumberSelected = 0,
                            AllowUnlimitedSelection = false,
                            AddButtonDisabled = false,
                            RemoveButtonDisabled = true,
                            styleForAddButton = ShMod.Settings.styleForButtonAvailable,
                            styleForRemoveButton = ShMod.Settings.styleForButonNotAvailable,
                            FilterVisible = false

                        };

                        tempList.Add(ug);
                }



            ////Before returning the data, we have to implement some changes to it based on the onlyReturnUnitsInCollection and rulesetId flags

            ////If using my collection, each ship or sub will be returned as a unique entity, so it can only be selected once

            List<ShMod.UnitForGameSystemDTO> listToReturn = new List<ShMod.UnitForGameSystemDTO>();

            foreach (var unit in tempList)
            {
                //Get a list of unique ship and subs if using local collection
                //if (onlyReturnUnitsInCollection == true && (unit.subUnitTypeObj.UnitTypeId == 1 || unit.subUnitTypeObj.UnitTypeId == 3))
                if (onlyReturnUnitsInCollection == true && (unit.subUnitTypeObj.UnitTypeId == 1 || unit.subUnitTypeObj.UnitTypeId == 3) && unit.ShipsSubsInClass != null)
                    {
                    List<string> shipList = new List<string>();
                    shipList = unit.ShipsSubsInClass.Split(",").ToList();

                    foreach (var ship in shipList)
                    {


                        //assign all details from returned server value
                        ShMod.UnitForGameSystemDTO newUnit = new ShMod.UnitForGameSystemDTO()
                        {
                            //TODO: The ID is problematic as it will be the same for multiple units
                            Id = unit.Id,
                            Name_ClassName = unit.Name_ClassName,
                            Countryobj = unit.Countryobj,
                            Alliance = unit.Alliance,
                            NumberinClass_shipSub = 1,
                            Cost = unit.Cost,
                            ImagePath = unit.ImagePath,
                            ShipsSubsInClass = ship.Trim(),
                            NumberSelected = 0,
                            AllowUnlimitedSelection = false,
                            AddButtonDisabled = false,
                            RemoveButtonDisabled = true,
                            styleForAddButton = ShMod.Settings.styleForButtonAvailable,
                            styleForRemoveButton = ShMod.Settings.styleForButonNotAvailable,
                            FilterVisible = false,
                            subUnitTypeObj = unit.subUnitTypeObj
                            //Country = unit.Country,
                            //SubUnitType = unit.SubUnitType,
                        };
                        Console.WriteLine("Adding "+ newUnit.ShipsSubsInClass);

                        listToReturn.Add(newUnit);
                    }

                }
                else
                {

                    unit.styleForAddButton = ShMod.Settings.styleForButtonAvailable;
                    unit.styleForRemoveButton = ShMod.Settings.styleForButonNotAvailable;
                    unit.AddButtonDisabled = false;
                    unit.RemoveButtonDisabled = true;
                    unit.FilterVisible = false;

                    //TODO: have to deal with the allowunlimited selection issue
                    //AllowUnlimitedSelection = false,
                    //TODO DOnt know how to deal with this
                    if(onlyReturnUnitsInCollection == false || (onlyReturnUnitsInCollection == true && unit.ShipsSubsInClass != null)) {
                        listToReturn.Add(unit);
                    }

                }
            }

            List<ShMod.UnitForGameSystemDTO> SortedList = listToReturn.OrderBy(o => o.subUnitTypeObj.SubUnitName).ToList();

            ShMod.ServiceResponse<List<ShMod.UnitForGameSystemDTO>> SrRespone = new ShMod.ServiceResponse<List<ShMod.UnitForGameSystemDTO>>()
            {
                //Data = listToReturn
                Data = SortedList
            };

            //TODO: UP TO HERE. Need a bit more processing to create duplicates and set the max amount of ships that can be selected. See below
            return SrRespone;


        }

        private async Task<string> ReturnAllianceName(int allianceID)
        {
            //This could be made an array of this service method to stop the call to the json file
            var alliances = await _http.GetFromJsonAsync<ShMod.Alliance[]>("Data/alliances.json");

            string name;
            foreach (var item in alliances)
            {
                if(item.Id == allianceID)
                {
                    return item.Name;
                }
            }

            return "Unknown alliance";
        }

        public async Task SetListOfUnits(int rulesetId, bool onlyReturnUnitsInCollection)
        {
            var resultUnits = await GetListofAllGameUnitsForRuleset(rulesetId, onlyReturnUnitsInCollection);
            UnitList.Clear();
            UnitList = resultUnits.Data;
            

        }

        public async Task<ShMod.ServiceResponse<List<ShMod.UnitWithGameSystemDetails>>> GetListofAllGameUnitsWithGameSpecDetails()
        {

            var gs = await _http.GetFromJsonAsync<ShMod.GameSystemUnitSpecificDetail[]>("Data/gameSystemSpecific.json");
            var units = await _http.GetFromJsonAsync<ShMod.Unit[]>("Data/units.json");
            //var countries = await _http.GetFromJsonAsync<Country[]>("Data/countries.json");
            List<ShMod.Unit> Units = units.ToList();
            //List<Country>cnt = GetListOfCountries().Result.Data;
            //List<SubUnitType> su =  GetListOfSubUnits().Result.Data;

            var responseCnt = await _http.GetAsync(($"Data/countries.json"));
            var jsonResultCnt = await responseCnt.Content.ReadFromJsonAsync<List<ShMod.Country>>();
            //Console.WriteLine("jsonResultCnt " + jsonResultCnt.ToString());
            List<ShMod.Country> cntries = jsonResultCnt.ToList();

            List<ShMod.SubUnitTypeDTO> su = new List<ShMod.SubUnitTypeDTO>();
            var responseSU = await GetListOfSubUnits();
            su = responseSU.Data;


            //TODO: The subunit obkect contains a unitype object which is null and a print colour object
            //may gave 



            // var responseSU = await _http.GetAsync(($"Data/subUnitTypes.json"));
            //var jsonResultSU = await responseSU.Content.ReadFromJsonAsync<List<SubUnitType>>();
            //Console.WriteLine("jsonResultSU" + jsonResultSU.ToString());
            //List<SubUnitType> su = jsonResultSU.ToList();

            //Console.WriteLine("number of sub units found = " + su.Count().ToString());

            //Console.WriteLine("number of countries found = " + cntries.Count().ToString());
            
            var query =
            from un in units
            join cntr in cntries on un.CountryId equals cntr.Id
            where cntr.Id ==un.CountryId
            select un;

            List <ShMod.UnitWithGameSystemDetails> returnList = new List<ShMod.UnitWithGameSystemDetails >();
            //foreach (var unit in units)
            foreach (var unit in query)
                {
                ShMod.UnitWithGameSystemDetails gssd = new ShMod.UnitWithGameSystemDetails();
                gssd.Id = unit.Id;
                //Console.WriteLine("ID = " + gssd.Id);
                gssd.Unit = unit;       //does this get the country object?
                //Console.WriteLine("Unit Name GOES HERE = " + gssd.Unit.Name_ClassName);

                gssd.Unit.Country = cntries.Find(x => x.Id == unit.CountryId);
                //Console.WriteLine("Country name  = " + unit.Country.CountryName);
                gssd.Unit.SubUnitType = new ShMod.SubUnitType();



                ShMod.SubUnitTypeDTO sudto = new ShMod.SubUnitTypeDTO();
                sudto = su.Find(x => x.Id == unit.SubUnitTypeId);
                gssd.Unit.SubUnitType.Id = sudto.Id;
                gssd.Unit.SubUnitType.UnitTypeId = sudto.UnitTypeId;
                gssd.Unit.SubUnitType.SubUnitName= sudto.SubUnitName;
                Color color = new();
                color = Color.FromArgb(255, sudto.RGBDetails.R, sudto.RGBDetails.G, sudto.RGBDetails.B);

                gssd.Unit.SubUnitType.PrintColour = color;
                ShMod.UnitType unitType = new ShMod.UnitType();
                unitType.Name = sudto.UnitTypeName;
                unitType.Id = sudto.UnitTypeId;
                gssd.Unit.SubUnitType.UnitType = unitType;

                //gssd.Unit.SubUnitType = su.Find(x => x.Id == unit.SubUnitTypeId);
                //Console.WriteLine(unit.Country.CountryName);
                //Console.WriteLine(unit.SubUnitType.SubUnitName);
                //Console.WriteLine(unit.Name_ClassName);
                //Console.WriteLine(unit.ShipsSubsInClass);

                gssd.RuleSetName = null;

                //TODO: this function is not getting a ruleset object in the response ITS stuffing up the unitsDB.razor
                List<ShMod.GameSystemUnitSpecificDetail> gsusd = new List<ShMod.GameSystemUnitSpecificDetail>();
                var responseGS = await GetGameSystemUnitSpecificDetails(unit.Id);
                gsusd = responseGS.Data;
                gssd.GameSpecificDetailList = gsusd;
                //Console.WriteLine("Count of gsusd = " + gsusd.Count().ToString());
                //Console.WriteLine("Count of gssd.GameSpecificDetailList = " + gssd.GameSpecificDetailList.Count().ToString());


                if (gsusd.Count() == 0)
                {
                    //Console.WriteLine("No  GameSystemUnitSpecificDetails found for " + gssd.Unit.Name_ClassName);
                    gssd.GameSpecificDetailList = new List<ShMod.GameSystemUnitSpecificDetail>();
                }


                //Console.WriteLine("GetGameSystemUnitSpecificDetails count = " + gsusd.Count().ToString());
                



                returnList.Add(gssd);
            }

            //var aTest =
            //    from gss in gs
            //    join u in Units on gss.UnitId equals u.Id
            //    join country in cnt on u.CountryId equals country.Id
            //    join subunits in su on u.SubUnitTypeId equals subunits.Id
            //    select
            //    new UnitWithGameSystemDetails(gss);

            //Console.WriteLine(returnList.Count() + " untis in gamesystemspefific found");
            if (returnList != null && returnList.Count != 0)
            {

                return new ShMod.ServiceResponse<List<ShMod.UnitWithGameSystemDetails>>
                {
                    Data = returnList
                };
            }
            else
            {
                return new ShMod.ServiceResponse<List<ShMod.UnitWithGameSystemDetails>>
                {
                    Data = new List<ShMod.UnitWithGameSystemDetails>(),
                    Success = false,
                    Message = "No ships found"
                };
            }
        }

        public async Task<ShMod.ServiceResponse<int>> UpdateUnit(ShMod.Unit unit)
        {
            //var result = await _http.PutAsJsonAsync("api/UnitDetails/updateUnit", unit);

            //return await result.Content.ReadFromJsonAsync<ShMod.ServiceResponse<int>>();

            //Have to read in the entire unit list and then de-serialize it so I can add the new item to the list and seriailze


            var UnitJS = await _http.GetFromJsonAsync<cutDownUnit[]>("Data/units.json");
            List<cutDownUnit> unitList = UnitJS.ToList();

            var simplteUnit = new cutDownUnit
            {
                Id = unit.Id,
                SubUnitTypeId = unit.SubUnitTypeId,
                CountryId = unit.CountryId,
                NumberinClass_shipSub = unit.NumberinClass_shipSub,
                Name_ClassName = unit.Name_ClassName,
                ShipsSubsInClass = unit.ShipsSubsInClass
            };


            //find the item in the list and replace it
            int index = unitList.FindIndex(item => item.Id == simplteUnit.Id);

            if (index != -1)
            {
                unitList[index] = simplteUnit;
            }


            string json = System.Text.Json.JsonSerializer.Serialize(unitList, new JsonSerializerOptions { WriteIndented = true });

            try
            {
                await System.IO.File.WriteAllTextAsync("E:\\ProjectsForGit\\ShipSel3\\ShipSel3\\wwwroot\\Data\\units.json", json);

                var sr = new ShMod.ServiceResponse<int>
                {
                    Message = "All good",
                    Success = true
                };
                return sr;

            }
            catch (Exception ex)
            {
                {
                    var sr = new ShMod.ServiceResponse<int>
                    {
                        Message = "The files weren't uploaded correctly. Image not added " + ex.Message,
                        Success = false
                    };
                    return sr;
                }
            }
        }

        public async Task<ShMod.ServiceResponse<bool>> DeleteUnit(int unitId)
        {
            //TODO: Not tested yet
            //var result = await _http.DeleteAsync("/api/unitDetails/" + unitId);

            //return await result.Content.ReadFromJsonAsync<ShMod.ServiceResponse<bool>>();

            //get a list
            var UnitJS = await _http.GetFromJsonAsync<cutDownUnit[]>("Data/units.json");
            List<cutDownUnit> unitList = UnitJS.ToList();

            int index = -1;
            index = unitList.FindIndex(item => item.Id == unitId);

            int countryID;

            if (index >= 0)
            {
                countryID = unitList[index].CountryId;
                unitList.RemoveAt(index);
            }
            else
            {

                var sr = new ShMod.ServiceResponse<bool>
                {
                    Message = "Couldnt find the item in the list",
                    Success = false
                };
                return sr;

            }

            string json = System.Text.Json.JsonSerializer.Serialize(unitList, new JsonSerializerOptions { WriteIndented = true });

            try
            {
                await System.IO.File.WriteAllTextAsync("E:\\ProjectsForGit\\ShipSel3\\ShipSel3\\wwwroot\\Data\\units.json", json);


                var sr = new ShMod.ServiceResponse<bool>
                {
                    Message = "All good",
                    Success = true
                };



                //get a list of gameSpecUnitDetails for the unit being deleted
                var jsGameSpecList = await _http.GetFromJsonAsync<cutDownGameSystemSpecific[]>("Data/gameSystemSpecific.json");
                List<cutDownGameSystemSpecific> gameSpecList = jsGameSpecList.ToList();

                foreach (var item in gameSpecList)
                {
                    if (item.UnitId == unitId)
                    {
                            await DeleteGameSystemUnitSpecifiDetails(item.Id, countryID, item.RulesetId);


                    }
                }
                return sr;

            }
            catch (Exception ex)
            {
                {
                    var sr = new ShMod.ServiceResponse<bool>
                    {
                        Message = "Couldnt serialise the collection " + ex.Message,
                        Success = false
                    };
                    return sr;
                }
            }

        }

        public async Task<ShMod.ServiceResponse<List<ShMod.OrderCard>>> GetBroadSideOrderCards()
        {
            var result = await _http.GetFromJsonAsync<ShMod.OrderCard[]>("Data/broadsides_order_cards.json");

            if (result != null && result.Length != 0)
            {

                return new ShMod.ServiceResponse<List<ShMod.OrderCard>>
                {
                    Data = result.ToList()
                };
            }
            else
            {
                return new ShMod.ServiceResponse<List<ShMod.OrderCard>>
                {
                    Data = new List<ShMod.OrderCard>(),
                    Success = false,
                    Message = "No order cards found"
                };
            }
        }

        public async Task<ShMod.ServiceResponse<List<ShMod.DamageCardData>>> GetBroadSideDamageCards()
        {
            var result = await _http.GetFromJsonAsync<ShMod.DamageCardData[]>("Data/broadsides_damage_cards.json");

            if (result != null && result.Length != 0)
            {

                return new ShMod.ServiceResponse<List<ShMod.DamageCardData>>
                {
                    Data = result.ToList()
                };
            }
            else
            {
                return new ShMod.ServiceResponse<List<ShMod.DamageCardData>>
                {
                    Data = new List<ShMod.DamageCardData>(),
                    Success = false,
                    Message = "No damage cards found"
                };
            }
        }

        public async Task<ShMod.ServiceResponse<bool>> DeleteGameSystemUnitSpecifiDetails(int gameSystemSpecId, int countryID, int rulesetId)
        {
            //method will go through the gameSystemSpecific.json file and delete any references that match the UnitId

            var jsGameSpecList = await _http.GetFromJsonAsync<cutDownGameSystemSpecific[]>("Data/gameSystemSpecific.json");
            List<cutDownGameSystemSpecific> gameSpecList = jsGameSpecList.ToList();

            int index = -1;
            index = gameSpecList.FindIndex(item => item.Id == gameSystemSpecId);

            
            if (index >= 0)
            {

                //delete the associate image too
                string pathToDelete = "E:\\ProjectsForGit\\ShipSel3\\ShipSel3\\wwwroot\\ShipImages\\" + rulesetId.ToString() + "\\" + countryID.ToString() + "\\"
                    + gameSpecList[index].ImagePath;

                gameSpecList.RemoveAt(index);


                File.Delete(pathToDelete);

            }
            else
            {
                var sr = new ShMod.ServiceResponse<bool>
                {
                    Message = "Couldnt find the item in the list",
                    Success = false
                };
                return sr;

            }

            string json = System.Text.Json.JsonSerializer.Serialize(gameSpecList, new JsonSerializerOptions { WriteIndented = true });

            try
            {
                await System.IO.File.WriteAllTextAsync("E:\\ProjectsForGit\\ShipSel3\\ShipSel3\\wwwroot\\Data\\gameSystemSpecific.json", json);


                var sr = new ShMod.ServiceResponse<bool>
                {
                    Message = "All good",
                    Success = true
                };

                return sr;
            }
            catch (Exception ex)
            {
                {
                    var sr = new ShMod.ServiceResponse<bool>
                    {
                        Message = "Couldnt re-serialise the collection " + ex.Message,
                        Success = false
                    };
                    return sr;
                }
            }
        }


    }

    class ColorConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(Color));
        }

        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            writer.WriteValue(((Color)value).ToArgb());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            return Color.FromArgb(Convert.ToInt32(reader.Value));
        }
    }

}
