using System;
using UnityEngine;

public static class MercenaryRoster
{
 [Serializable]public class Company {public string Name,Captain,Tradition,Port;public int Infantry,Archers,Cavalry,Ships,Engineers,Cannons,Days,Risk,Gold;public bool Sanctuary,Order;
 public int Land=>Infantry+Archers+Cavalry;public bool Flotilla=>Ships>0;
 public string Package=>Infantry+" infantry · "+Archers+" archers · "+Cavalry+" cavalry"+(Ships>0?" · "+Ships+" warships with crews":"")+(Engineers>0?" · "+Engineers+" engineers":"")+(Cannons>0?" · "+Cannons+" crewed cannons":"");
 public Company(string name,string captain,string tradition,string port,int days,int i,int a,int c,int s,double multiplier,int risk=0,int engineers=0,int cannons=0,bool sanctuary=false,bool order=false){Name=name;Captain=captain;Tradition=tradition;Port=port;Days=days;Infantry=i;Archers=a;Cavalry=c;Ships=s;Risk=risk;Engineers=engineers;Cannons=cannons;Sanctuary=sanctuary;Order=order;
 // Full replacement package: regulars 10/20/100, hull 1000 and cannon 500,
 // engineer 250; ship/cannon include their operational crews and materials.
 Gold=(int)(Math.Ceiling(((i*10+a*20+c*100)+(s*(1000+SpecialistPersonnel.ShipOrdinaryCrew*10+250+SpecialistPersonnel.ShipMaterials*10))+(cannons*(500+SpecialistPersonnel.CannonOrdinaryCrew*10+250+SpecialistPersonnel.CannonMaterials*10))+engineers*250)*multiplier/2)*2);if(sanctuary)Gold=1000;}
 }
 public static readonly Company[] All={
 new Company("The Emir’s Exiles","Yusuf al-Burji","Circassian Mamluk household","Cyprus",32,0,80,120,0,1.45),
 new Company("The Red Oars","Pere de Roca","Catalan mariners","Malta",30,180,80,0,2,1.30,5),
 new Company("The Messina Brotherhood","Matteo di Messina","Sicilian maritime infantry","Malta",26,220,60,0,2,1.25),
 new Company("The White Cross","Hugues de Montclair","Savoyard men-at-arms","Venice",45,160,0,100,0,1.35),
 new Company("The Steppe Falcons","Temir of the Grey Mare","Crimean Tatar horsemen","Caffa",20,0,150,150,0,1.20,20),
 new Company("The Ashen Standard","Ali of the Ashen Tent","Turkmen retainers","Cyprus",35,0,80,180,0,1.20,10),
 new Company("The Tunisian Crescent","Ibrahim al-Bahri","Tunisian corsairs","Malta",38,140,80,0,3,1.25,15),
 new Company("The Mountain Spurs","Nikola Vuković","Balkan light cavalry","Venice",32,50,0,200,0,1.25,5),
 new Company("The Candian Bow","Manuel Kalergis","Cretan archers","Venice",28,40,260,0,0,1.30),
 new Company("The Crossbows of San Giorgio","Luca Spinola","Ligurian crossbowmen","Malta",36,80,300,0,0,1.35),
 new Company("The Alpine Pike","Ulrich von Stein","Swiss infantry","Venice",50,400,0,0,0,1.40),
 new Company("Company of the Grey Lance","Rinaldo da Monte","Condottieri · mounted retinue","Venice",42,100,0,160,0,1.45),
 new Company("Company of the Broken Bridge","Marco del Ponte","Condottieri · siege veterans","Venice",34,250,150,0,0,1.25,5),
 new Company("Company of the Black Purse","Bartolomeo Nero","Condottieri · mixed company","Malta",40,120,80,100,0,1.20,25),
 new Company("The Iberian Lances","Giorgi of the High Pass","Georgian noble following","Caffa",30,100,80,120,0,1.35),
 new Company("The Bohemian Fellowship","Jan of the Mill","Bohemian infantry and crossbows","Venice",48,280,120,0,0,1.30,5),
 new Company("The Adriatic Brotherhood","Maro of the Islands","Adriatic mariners","Venice",24,120,80,0,2,1.30),
 new Company("The Moreote Exiles","Andronikos of the South","Moreote retainers and households","Malta",22,180,60,60,0,1.25,0,0,0,true),
 new Company("The Rhodian Detachment","Brother Antoine","Hospitaller sergeants and sailors","Cyprus",18,120,60,0,1,1.25,0,0,0,false,true),
 new Company("The Master’s Train","Master Paolo","Gun founders and armed escort","Venice",45,100,0,0,0,1.50,0,10,2),
 new Company("The Kuban Household","Aslan of the Ford","Circassian retainers · Copa","Caffa",20,120,100,60,0,1.35),
 new Company("The Mangup Retinue","The captain of Mangup","Theodoro cavalry","Caffa",24,0,0,80,0,1.35){Gold=10800}
 };
}
