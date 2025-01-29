using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using SharedLibrary.LibraryServices.libraryLocalStorageService;
using SharedLibrary.Services.UnitsandListsServiceClient;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace SharedLibrary.Models

{

    public class BroadsideSDS
    {



        [JsonIgnore]
        public ILocalStorageFromLibrary _localStorage { get; set; }

        public BroadsideSDS(ILocalStorageFromLibrary localStorageFromLibrary )
        {

            _localStorage = localStorageFromLibrary;
            Random rnd = new Random();


            id = DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + rnd.Next(0, 1000000).ToString();


        }

        public BroadsideSDS()
        {
            
        }
        private int hullhits;

        [JsonInclude]
        public int HullHits
        {
            get
            {
                return hullhits;
            }
            set
            {
                hullhits = value;
                setSummaryText();

                //updateLocalStorage();

            }
        }

        private int floodingHits;
        
        [JsonInclude]
        public int FloodingHits
        {
            get { return floodingHits; }
            set
            {
                floodingHits = value;
                setSummaryText();
            }
        }

        private string shipName;

        [JsonInclude]
        public string ShipName
        {
            get { return shipName; }
            set
            {
                shipName = value;
                setSummaryText();
            }
        }

        private int currentSpeed;

        [JsonInclude]
        public int CurrentSpeed
        {
            get { return currentSpeed; }
            set
            {
                currentSpeed = value;
                setSummaryText();
            }
        }

        private string directedGunTag;

        [JsonInclude]
        public string DirectedGunTag
        {
            get { return directedGunTag; }
            set
            {
                directedGunTag = value;
                setSummaryText();
            }
        }

        //public string ShipName { get; set; }
        [JsonInclude]
        public string ImagePath { get; set; }
        //public int CurrentSpeed { get; set; } = 0;

        //public int FloodingHits { get; set; } = 0;
        [JsonInclude]
        public List<DamageCard> DamageCards { get; set; } = new List<DamageCard>();

        [JsonInclude]
        public int selectedOrderCard { get; set; } = 0;
        [JsonInclude]
        public string selectedOrderImagePath { get; set; } = string.Empty;
        [JsonInclude]
        public bool orderIsActive { get; set; } = false;
        [JsonInclude]
        public bool torpsFired { get; set; } = false;
        [JsonInclude]
        public bool starTorpsFired { get; set; } = false;
        [JsonInclude]
        public bool portTorpsFired { get; set; } = false;
        [JsonInclude]
        public bool renameHidden { get; set; } = true;

        [JsonInclude]
        public string summaryText { get; set; } = string.Empty;

        [JsonInclude]
        public string id { get; set; }

        [JsonInclude]
        public int unitId { get; set; }

        public void setSummaryText()
        {
            summaryText = "S:" + CurrentSpeed.ToString() + " HH:" + HullHits.ToString() + " F:" + FloodingHits.ToString() + " DC:" + DamageCards.Count.ToString() + " ID:" + DirectedGunTag;
        }

        //public async void updateLocalStorage()
        //{

        //    System.Diagnostics.Debugger.Break();  ServiceResponse<bool> vsrReturn = await _localStorage.UpdateIndividualSDS(this);


        //    if (vsrReturn != null)
        //    {

        //    }


        //}
    }

    public class DamageCardData
    {
        //This is the source card from the JSON. 
        public string Name { get; set; }
        public string ImagePath { get; set; }
        public int id { get; set; }
        public int number_cards { get; set; }
        public string card_text { get; set; }
    }

    
    public class DamageCard
    {
        //This is the card object that goes into a deck
        [JsonInclude]
        public string Name { get; set; }
        [JsonInclude]
        public string ImagePath { get; set; }
        [JsonInclude]
        public int id { get; set; }
        [JsonInclude]
        public string card_text { get; set; }
    }

    public class OrderCard
    {
        //can only 1 Order card
        public string Name { get; set; }
        public string ImagePath { get; set; }
        public int id { get; set; }
    }



}
