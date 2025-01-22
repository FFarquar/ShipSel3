//using NavalGame.Client.Services.UploadDownloadService;
using ShipSel3.Models;
using ShipSel3.Shared;
using System.Net.Http.Json;
using ShipSel3.Models;
using System.Text.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Drawing;
using ShipSel3.Services.UploadDownloadService;
using Microsoft.EntityFrameworkCore;
using ShipSel3.Pages;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.IO;
using System.Text.Json;
using System.Runtime.CompilerServices;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace ShipSel3.Services.UnitsandListsServiceClient
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
        public List<SH.UnitForGameSystemDTO> UnitList { get; set; } = new List<SH.UnitForGameSystemDTO>(); //a list that contains all units. The printpage will have access to this

        private class cutDownUnit
        {
            public int Id { get; set; }
            public int SubInitTypeId { get; set; }
            public int CountryId { get; set; }
            public int NumberinClass_shipSub { get; set; }
            public string Name_ClassName { get; set; } = string.Empty;
            public string ShipsSubsInClass { get; set; } = string.Empty;
        }
        public async Task<SH.ServiceResponse<int>> AddUnit(SH.Unit unitToAdd)
        {


            //TODO this isnt working. Not suree how to construct the method to write to the json file

            var simplteUnit = new cutDownUnit
            {
                Id = 1000,
                SubInitTypeId = unitToAdd.SubUnitTypeId,
                CountryId = unitToAdd.CountryId,
                NumberinClass_shipSub = unitToAdd.NumberinClass_shipSub,
                Name_ClassName = unitToAdd.Name_ClassName,
                ShipsSubsInClass = unitToAdd.ShipsSubsInClass
            };

            //var UnitJS = await _http.GetFromJsonAsync<Unit[]>("Data/units.json");
            //List<Unit> unitList = UnitJS.ToList();      //test to see if I can ge a response from theJSON

            ////////
            ///
            string jsonObject = System.Text.Json.JsonSerializer.Serialize(simplteUnit);
            //TextWriter writer;
            //using (writer = new StreamWriter(@"E:\ProjectsForGit\ShipSel3\ShipSel3\wwwroot\Data\units.json", append: true))
            //{
            //    writer.WriteLine(jsonObject);
            //}


            //using (StreamWriter outputFile = new StreamWriter(@"E:\ProjectsForGit\ShipSel3\ShipSel3\wwwroot\Data\units.json",append:false))
            //using (StreamWriter outputFile = new StreamWriter(@"C:\Temp\units.json", append: false))
            //{
            //    await outputFile.WriteAsync(jsonObject);
            //}



            //string jsonObject = System.Text.Json.JsonSerializer.Serialize(simplteUnit);
            //var content = new StringContent(jsonObject, Encoding.UTF8, "application/json");


            //var response = await _http.PutAsync("/Data/units.json", content);

            //if (response.IsSuccessStatusCode) Console.Write("Success");
            //else Console.Write("Error");
            ////////




            return new SH.ServiceResponse<int>();

            //var result = await _http.PostAsJsonAsync($"data/units.json", jsonString);

            //  return await result.Content.ReadFromJsonAsync<ServiceResponse<int>>();
        }

        public async Task<SH.ServiceResponse<int>> AddGameSystemUnitSpecificDetail(SH.GameSystemUnitSpecificDetail gamespefic, List<FileUploadDTO> browserFiles, int countryId)
        {

            var filesUpload = new SH.ServiceResponse<List<UploadResult>>();
            if (browserFiles.Count > 0)
            {
                filesUpload = await _UDSC.UploadFiles(browserFiles, gamespefic.RulesetId, countryId);
                if (!filesUpload.Success)
                {
                    //the files weren't uploaded
                    var sr = new SH.ServiceResponse<int>
                    {
                        Message = "The files weren't uploaded correctly. Image not added " + filesUpload.Message,
                        Success = false
                    };
                    return sr;
                }
            }

            var result = await _http.PostAsJsonAsync("api/unitdetails/addgamespecificDetailsForUnit", gamespefic);

            return await result.Content.ReadFromJsonAsync<SH.ServiceResponse<int>>();
        }

        public async Task<SH.ServiceResponse<List<SH.Country>>> GetListOfCountriesForSelectedUnitsInGameSystem(int ruleSetId)
        {
            //this function will get a list of countries that have actually had units assigned

            SH.ServiceResponse<List<SH.Country>> fullCountryList = await GetListOfCountries();

            var randomid = Guid.NewGuid().ToString();
            var gameSystemSpecificURL = $"Data/gameSystemSpecific.json?{randomid}";
            var GameSystemUnitJs = await _http.GetFromJsonAsync<SH.GameSystemUnitSpecificDetail[]>(gameSystemSpecificURL);
            //var GameSystemUnitJs = await _http.GetFromJsonAsync<GameSystemUnitSpecificDetail[]>("Data/gameSystemSpecific.json");
            var UnitJS = await _http.GetFromJsonAsync<SH.Unit[]>("Data/units.json");
            List<SH.GameSystemUnitSpecificDetail> gameSpecList = GameSystemUnitJs.ToList();
            List<SH.Unit> unitList = UnitJS.ToList();

            List<SH.Country> filteredCountryList = new List<SH.Country>();
            //TODO: This query works well when joining 2 tables and usign a where clause to filter selections
            foreach (var c in fullCountryList.Data)
            {
                var query = unitList
                                .Join(gameSpecList,
                                    ut => ut.Id,
                                    gsu => gsu.UnitId,
                                    (ut, gsu) => new { Unit = ut, GameSystemUnitSpecificDetail = gsu })
                                .Where(x => x.Unit.CountryId == c.Id && x.GameSystemUnitSpecificDetail.RulesetId == ruleSetId);


                if (query.Count() > 0)
                {
                    filteredCountryList.Add(c);
                }
            }

            return new SH.ServiceResponse<List<SH.Country>>
            {
                Data = filteredCountryList
            };
        }

        public async Task<SH.ServiceResponse<List<SH.Country>>> GetListOfCountries()
        {
            var result = await _http.GetFromJsonAsync<SH.Country[]>("Data/countries.json");

            if (result != null && result.Length != 0)
            {
                return new SH.ServiceResponse<List<SH.Country>>
                {
                    Data = result.ToList()
                };
            }
            else
            {
                return new SH.ServiceResponse<List<SH.Country>>
                {
                    Data = new List<SH.Country>(),
                    Success = false,
                    Message = "No countries found"
                };

            }
            //            var result = await _http.GetFromJsonAsync<ServiceResponse<List<Country>>>("api/unitdetails/countries");
            //            return result;
        }

        public async Task<SH.ServiceResponse<List<SH.SubUnitTypeDTO>>> GetListOfSubUnits()
        {

            var rawSubunit = await _http.GetStringAsync("Data/subUnitTypes.json");
            var subUnits = JsonConvert.DeserializeObject<List<SH.SubUnitType>>(rawSubunit,

               new JsonSerializerSettings
               {
                   NullValueHandling = NullValueHandling.Ignore,
                   Converters =
                   {
                       new ColorConverter()
                   }
               });

            //Need to add the unit type object for subunit

            var unitTypesJS = await _http.GetFromJsonAsync<SH.UnitType[]>("Data/unitTypes.json");

            List<SH.UnitType> unitTypeList = unitTypesJS.ToList();

            var _includeUnitType = subUnits
                    .Join(unitTypeList,
                        su => su.UnitTypeId,
                            ut => ut.Id,
                                (su, ut) => new { SubUnitType = su, UnitType = ut });

            //if (subUnits != null && subUnits.Count > 0)
            if (_includeUnitType != null)

            {
                List<SH.SubUnitTypeDTO> subunitresponse = new List<SH.SubUnitTypeDTO>();
                foreach (var item in _includeUnitType)
                {
                    var subunit = new SH.SubUnitTypeDTO
                    {
                        Id = item.SubUnitType.Id,
                        SubUnitName = item.SubUnitType.SubUnitName,
                        UnitTypeName = item.UnitType.Name,
                        UnitTypeId = item.UnitType.Id,
                        RGBDetails = new SH.RGBDetails()
                        {
                            R = item.SubUnitType.PrintColour.R,
                            G = item.SubUnitType.PrintColour.G,
                            B = item.SubUnitType.PrintColour.B,
                        }
                    };
                    subunitresponse.Add(subunit);
                }

                return new SH.ServiceResponse<List<SH.SubUnitTypeDTO>>
                {
                    Data = subunitresponse
                };


            }
            else
            {
                return new SH.ServiceResponse<List<SH.SubUnitTypeDTO>>
                {
                    Data = new List<SH.SubUnitTypeDTO>(),
                    Success = false,
                    Message = "No Sub units found"
                };

            }
            //var result = await _http.GetFromJsonAsync<ServiceResponse<List<SubUnitTypeDTO>>>("api/unitdetails/subunits");
            //return result;


        }

        public async Task<SH.ServiceResponse<List<SH.UnitType>>> GetListOfUnitTypes()
        {

            var result = await _http.GetFromJsonAsync<SH.UnitType[]>("Data/unitTypes.json");

            if (result != null && result.Length != 0)
            {
                return new SH.ServiceResponse<List<SH.UnitType>>
                {
                    Data = result.ToList()
                };
            }
            else
            {
                return new SH.ServiceResponse<List<SH.UnitType>>
                {
                    Data = new List<SH.UnitType>(),
                    Success = false,
                    Message = "No Unit types found"
                };

            }
            //var result = await _http.GetFromJsonAsync<ServiceResponse<List<UnitType>>>("api/unitdetails/unittypes");
            //return result;
        }

        public async Task<SH.ServiceResponse<List<SH.RuleSet>>> GetRuleSets()
        {
            //var result = await _http.GetFromJsonAsync<RuleSet[]>("Data/ruleSets.json");
            //Console.WriteLine("resulet " + result);
            //List<RuleSet> rs = result.ToList();

            //Console.WriteLine("Rule set cout = " + rs.Count);

            var response = await _http.GetAsync(($"Data/ruleSets.json"));
            var jsonResult = await response.Content.ReadFromJsonAsync<List<SH.RuleSet>>();
            Console.WriteLine("jsonResult " + jsonResult.ToString());
            List<SH.RuleSet> rs = jsonResult.ToList();

            //if (result != null && result.Length != 0)
            if (jsonResult != null && rs.Count != 0)
            {
                return new SH.ServiceResponse<List<SH.RuleSet>>
                {
                    Data = rs,
                    Message = "all good"
                };
            }
            else
            {
                return new SH.ServiceResponse<List<SH.RuleSet>>
                {
                    Data = new List<SH.RuleSet>(),
                    Success = false,
                    Message = "No rulesets found"
                };

            }

        }

        public async Task<SH.ServiceResponse<SH.Unit>> GetUnitWithoutChildObjects(int unitId)
        {
            var result = await _http.GetFromJsonAsync<SH.ServiceResponse<SH.Unit>>("api/unitdetails/getUnitNoChildObjects/" + unitId);
            return result;
        }

        public async Task<SH.ServiceResponse<List<SH.GameSystemUnitSpecificDetail>>> GetGameSystemUnitSpecificDetails(int unitId)
        {
            //Cant call the function to get a list of rule sets
            List<SH.RuleSet> rs = new List<SH.RuleSet>();
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

            var GameSystemUnitJs = await _http.GetFromJsonAsync<SH.GameSystemUnitSpecificDetail[]>("Data/gameSystemSpecific.json");
            //Console.WriteLine("RS count  after next data read " + rs.Count.ToString());

            //Console.WriteLine("GameSystems  count = {0} id of first ruleset = {1} rulesetname = {2}", rs.Count().ToString(), rs[1].Id.ToString(), rs[1].RulesetName);

            //get a list of rule systems this unit is a part of

            var query =
                from gss in GameSystemUnitJs
                join rules in rs on gss.RulesetId equals rules.Id
                where gss.UnitId == unitId
                select gss;

            //Console.WriteLine("Query in GetGameSystemUnitSpecificDetails =  " + query.Count());

            List<SH.GameSystemUnitSpecificDetail> result = new List<SH.GameSystemUnitSpecificDetail>();
            foreach (SH.GameSystemUnitSpecificDetail item in query)
            {
                SH.GameSystemUnitSpecificDetail gssd = new SH.GameSystemUnitSpecificDetail();
                gssd.Cost = item.Cost;
                gssd.Id = item.Id;
                gssd.ImagePath = item.ImagePath;
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
                return new SH.ServiceResponse<List<SH.GameSystemUnitSpecificDetail>>
                {
                    Data = result
                };
            }
            else
            {
                return new SH.ServiceResponse<List<SH.GameSystemUnitSpecificDetail>>
                {
                    Data = new List<SH.GameSystemUnitSpecificDetail>(),
                    Success = false,
                    Message = "No gamesec found"
                };

            }
            //var result = await _http.GetFromJsonAsync<ServiceResponse<List<GameSystemUnitSpecificDetail>>>("api/unitdetails/getGameSystemUnitSpecificDetails/" + unitId);
            //return result;

        }

        public async Task<SH.ServiceResponse<SH.RuleSet>> GetRuleSet(int rulesetId)
        {

            var result = await _http.GetFromJsonAsync<SH.RuleSet[]>("Data/ruleSets.json");

            if (result != null && result.Length != 0)
            {
                var rs = result
                    .Where(item => item.Id == rulesetId)
                    .FirstOrDefault();
                return new SH.ServiceResponse<SH.RuleSet>
                {
                    Data = rs,
                };
            }
            else
            {
                return new SH.ServiceResponse<SH.RuleSet>
                {
                    Data = new SH.RuleSet(),
                    Success = false,
                    Message = "No rulesets found"
                };

            }

            //var result = await _http.GetFromJsonAsync<ServiceResponse<RuleSet>>("api/UnitDetails/getRuleSet/" + rulesetId);
            //return result;
            //asd
        }

        public async Task<SH.ServiceResponse<List<SH.UnitForGameSystemDTO>>> GetListofAllGameUnitsForRuleset(int rulesetId, bool onlyReturnUnitsInCollection)
        {
            //var result = await _http.GetFromJsonAsync<ServiceResponse<List<UnitForGameSystemDTO>>>("api/unitdetails/allunitsbyGameSystemandType/" + rulesetId + "/" + onlyReturnUnitsInCollection);
            var CountriesJs = await _http.GetFromJsonAsync<SH.Country[]>("Data/countries.json");
            var UnitsJs = await _http.GetFromJsonAsync<SH.Unit[]>("Data/units.json");

            var rawSubunit = await _http.GetStringAsync("Data/subUnitTypes.json");
            //  var message = JsonConvert.DeserializeObject<List<SubUnitType>>(rawSubunit);

            var SubUnitTypesJs = JsonConvert.DeserializeObject<List<SH.SubUnitType>>(rawSubunit,
               new JsonSerializerSettings
               {
                   NullValueHandling = NullValueHandling.Ignore,
                   Converters =
                   {
                       new ColorConverter()
                   }
               });

            //TODO: Is the colour correct? Does it need to be 64bit int

            var GameSystemUnitJs = await _http.GetFromJsonAsync<SH.GameSystemUnitSpecificDetail[]>("Data/gameSystemSpecific.json");

            List<SH.GameSystemUnitSpecificDetail> gameSpecList = GameSystemUnitJs.ToList();
            List<SH.Country> countryList = CountriesJs.ToList();
            List<SH.Unit> unitList = UnitsJs.ToList();
            List<SH.SubUnitType> subUnitList = SubUnitTypesJs.ToList();

            //Get a GameSystemUnitSpecificDetail object with the Unit object
            //var _GSWithUnit = GameSystemUnitJs
            //        .Join(UnitsJs,
            //                gsu => gsu.UnitId,
            //                    u => u.Id,
            //                        (gsu, u) => new { GameSystemUnitSpecificDetail = gsu, Unit = u});

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

            List<SH.UnitForGameSystemDTO> tempList = new List<SH.UnitForGameSystemDTO>();
            foreach (var item in _includeCountries)
            {
                var ShipsSubsInClassCleaned = item.GameSystemUnitSpecificDetail.GameSystemUnitSpecificDetail.Unit.ShipsSubsInClass;
                if (ShipsSubsInClassCleaned != null)
                {
                    ShipsSubsInClassCleaned = ShipsSubsInClassCleaned.Trim();
                }
                SH.UnitForGameSystemDTO ug = new SH.UnitForGameSystemDTO()
                {
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
                    styleForAddButton = SH.Settings.styleForButtonAvailable,
                    styleForRemoveButton = SH.Settings.styleForButonNotAvailable,
                    FilterVisible = false

                };

                tempList.Add(ug);
            }



            ////Before returning the data, we have to implement some changes to it based on the onlyReturnUnitsInCollection and rulesetId flags

            ////If using my collection, each ship or sub will be returned as a unique entity, so it can only be selected once

            List<SH.UnitForGameSystemDTO> listToReturn = new List<SH.UnitForGameSystemDTO>();

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
                        SH.UnitForGameSystemDTO newUnit = new SH.UnitForGameSystemDTO()
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
                            styleForAddButton = SH.Settings.styleForButtonAvailable,
                            styleForRemoveButton = SH.Settings.styleForButonNotAvailable,
                            FilterVisible = false,
                            subUnitTypeObj = unit.subUnitTypeObj
                            //Country = unit.Country,
                            //SubUnitType = unit.SubUnitType,
                        };
                        Console.WriteLine("Adding " + newUnit.ShipsSubsInClass);

                        listToReturn.Add(newUnit);
                    }

                }
                else
                {

                    unit.styleForAddButton = SH.Settings.styleForButtonAvailable;
                    unit.styleForRemoveButton = SH.Settings.styleForButonNotAvailable;
                    unit.AddButtonDisabled = false;
                    unit.RemoveButtonDisabled = true;
                    unit.FilterVisible = false;

                    //TODO: have to deal with the allowunlimited selection issue
                    //AllowUnlimitedSelection = false,
                    //TODO DOnt know how to deal with this
                    if (onlyReturnUnitsInCollection == false || (onlyReturnUnitsInCollection == true && unit.ShipsSubsInClass != null))
                    {
                        listToReturn.Add(unit);
                    }

                }
            }

            List<SH.UnitForGameSystemDTO> SortedList = listToReturn.OrderBy(o => o.subUnitTypeObj.SubUnitName).ToList();

            SH.ServiceResponse<List<SH.UnitForGameSystemDTO>> SrRespone = new SH.ServiceResponse<List<SH.UnitForGameSystemDTO>>()
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
            var alliances = await _http.GetFromJsonAsync<SH.Alliance[]>("Data/alliances.json");

            string name;
            foreach (var item in alliances)
            {
                if (item.Id == allianceID)
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

        public async Task<SH.ServiceResponse<List<SH.UnitWithGameSystemDetails>>> GetListofAllGameUnitsWithGameSpecDetails()
        {

            var gs = await _http.GetFromJsonAsync<SH.GameSystemUnitSpecificDetail[]>("Data/gameSystemSpecific.json");
            var units = await _http.GetFromJsonAsync<SH.Unit[]>("Data/units.json");
            //var countries = await _http.GetFromJsonAsync<Country[]>("Data/countries.json");
            List<SH.Unit> Units = units.ToList();
            //List<Country>cnt = GetListOfCountries().Result.Data;
            //List<SubUnitType> su =  GetListOfSubUnits().Result.Data;

            var responseCnt = await _http.GetAsync(($"Data/countries.json"));
            var jsonResultCnt = await responseCnt.Content.ReadFromJsonAsync<List<SH.Country>>();
            //Console.WriteLine("jsonResultCnt " + jsonResultCnt.ToString());
            List<SH.Country> cntries = jsonResultCnt.ToList();

            List<SH.SubUnitTypeDTO> su = new List<SH.SubUnitTypeDTO>();
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
            where cntr.Id == un.CountryId
            select un;

            List<SH.UnitWithGameSystemDetails> returnList = new List<SH.UnitWithGameSystemDetails>();
            //foreach (var unit in units)
            foreach (var unit in query)
            {
                SH.UnitWithGameSystemDetails gssd = new SH.UnitWithGameSystemDetails();
                gssd.Id = unit.Id;
                //Console.WriteLine("ID = " + gssd.Id);
                gssd.Unit = unit;       //does this get the country object?
                //Console.WriteLine("Unit Name GOES HERE = " + gssd.Unit.Name_ClassName);

                gssd.Unit.Country = cntries.Find(x => x.Id == unit.CountryId);
                //Console.WriteLine("Country name  = " + unit.Country.CountryName);
                gssd.Unit.SubUnitType = new SH.SubUnitType();



                SH.SubUnitTypeDTO sudto = new SH.SubUnitTypeDTO();
                sudto = su.Find(x => x.Id == unit.SubUnitTypeId);
                gssd.Unit.SubUnitType.Id = sudto.Id;
                gssd.Unit.SubUnitType.UnitTypeId = sudto.UnitTypeId;
                gssd.Unit.SubUnitType.SubUnitName = sudto.SubUnitName;
                Color color = new();
                color = Color.FromArgb(255, sudto.RGBDetails.R, sudto.RGBDetails.G, sudto.RGBDetails.B);

                gssd.Unit.SubUnitType.PrintColour = color;
                SH.UnitType unitType = new SH.UnitType();
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
                List<SH.GameSystemUnitSpecificDetail> gsusd = new List<SH.GameSystemUnitSpecificDetail>();
                var responseGS = await GetGameSystemUnitSpecificDetails(unit.Id);
                gsusd = responseGS.Data;
                gssd.GameSpecificDetailList = gsusd;
                //Console.WriteLine("Count of gsusd = " + gsusd.Count().ToString());
                //Console.WriteLine("Count of gssd.GameSpecificDetailList = " + gssd.GameSpecificDetailList.Count().ToString());


                if (gsusd.Count() == 0)
                {
                    //Console.WriteLine("No  GameSystemUnitSpecificDetails found for " + gssd.Unit.Name_ClassName);
                    gssd.GameSpecificDetailList = new List<SH.GameSystemUnitSpecificDetail>();
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

                return new SH.ServiceResponse<List<SH.UnitWithGameSystemDetails>>
                {
                    Data = returnList
                };
            }
            else
            {
                return new SH.ServiceResponse<List<SH.UnitWithGameSystemDetails>>
                {
                    Data = new List<SH.UnitWithGameSystemDetails>(),
                    Success = false,
                    Message = "No ships found"
                };
            }
        }

        public async Task<SH.ServiceResponse<int>> UpdateUnit(SH.Unit unit)
        {
            var result = await _http.PutAsJsonAsync("api/UnitDetails/updateUnit", unit);

            return await result.Content.ReadFromJsonAsync<SH.ServiceResponse<int>>();

        }

        public async Task<SH.ServiceResponse<bool>> DeleteUnit(int unitId)
        {
            //TODO: Not tested yet
            var result = await _http.DeleteAsync("/api/unitDetails/" + unitId);

            return await result.Content.ReadFromJsonAsync<SH.ServiceResponse<bool>>();
        }

        public async Task<SH.ServiceResponse<List<SH.OrderCard>>> GetBroadSideOrderCards()
        {
            var result = await _http.GetFromJsonAsync<SH.OrderCard[]>("Data/broadsides_order_cards.json");

            if (result != null && result.Length != 0)
            {

                return new SH.ServiceResponse<List<SH.OrderCard>>
                {
                    Data = result.ToList()
                };
            }
            else
            {
                return new SH.ServiceResponse<List<SH.OrderCard>>
                {
                    Data = new List<SH.OrderCard>(),
                    Success = false,
                    Message = "No order cards found"
                };
            }
        }

        public async Task<SH.ServiceResponse<List<SH.DamageCardData>>> GetBroadSideDamageCards()
        {
            var result = await _http.GetFromJsonAsync<SH.DamageCardData[]>("Data/broadsides_damage_cards.json");

            if (result != null && result.Length != 0)
            {

                return new SH.ServiceResponse<List<SH.DamageCardData>>
                {
                    Data = result.ToList()
                };
            }
            else
            {
                return new SH.ServiceResponse<List<SH.DamageCardData>>
                {
                    Data = new List<SH.DamageCardData>(),
                    Success = false,
                    Message = "No damage cards found"
                };
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
