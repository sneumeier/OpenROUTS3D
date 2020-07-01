using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SUMOConnectionScripts
{
    public enum VehicleClasses
    {
        privat,         //may drive on all lanes regardless of set permission  // default private 
        emergency,
        authority,
        army,
        vip,
        pedestrian,     // lanes which only allow this class are considered to be 'sidewalks' in NETCONVERT
        passenger,      // This is the default vehicle class and denotes regular passenger traffic
        hov,
        taxi,
        bus,            // Urban traffic line
        coach,          // overland transport
        delivery,       // Allowed on service roads which are not meant for public transport
        truck,
        trailer,        // truck with trailer
        motorcycle,
        moped,          // motorized 2-wheeler which may not drive on motorways
        bicycle,
        evehicle,       // future mobility concepts such as eletric vehicles may get special access
        tram,
        rail_urban,     // heavier than 'tram' but distinct from 'rail'. Encompasses Light Rail and S-Bahn
        rail,           // heavy rail
        rail_electric,  // heavy rail vehicle that may only drive on electrified tracks
        rail_fast,      // High-speed-rail
        ship,           // basic class for navigating waterways
        custom1,        // reserved for user-defined semantics
        custom2,        // reserved for user-defined semantics
        error = 999,
    }

    public class VehicleClass {

        private readonly string enumprivate = "private"; // try to make a workaround here
        public string Enumprivate
        {
            get { return enumprivate; }
        }
        

       

        public VehicleClass() { }

        /// <summary>
        /// Parses a given string into a enum object
        /// </summary>
        /// <param name="vehiclename"> string of type enum </param>
        /// <returns>Enum vehicleclasses</returns>
        public VehicleClasses ParseVehicleClassEnum(string vehiclename)
        {
            if(vehiclename.Equals(Enumprivate))
            {
                return ((VehicleClasses)Enum.Parse(typeof(VehicleClasses), "privat"));
            }
            try
            {
                return ((VehicleClasses)Enum.Parse(typeof(VehicleClasses), vehiclename));
            }catch(ArgumentException)
            {
                return VehicleClasses.error;
            }
        }

  



    }
}
