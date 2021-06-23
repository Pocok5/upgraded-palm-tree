using CsvHelper;
using CsvHelper.Configuration.Attributes;
using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace sundy
{
    internal class CsvRecord
    {
        [Name("num_kills")]
        public int NumKills { get; set; }
        [Name("weapon_name")]
        public string WeaponName { get; set; }
        [Name("attacker_weapon_id")]
        public long AttackerWeaponId { get; set; }
        [Name("vehicle")]
        public string Vehicle { get; set; }
        [Name("vehicle_slot_id")]
        public int? VehicleSlotId { get; set; }
        [Name("weapon_faction")]
        public string WeaponFaction { get; set; }
        [Name("target")]
        public string Target { get; set; }

        [Ignore]
        public bool NotVehicle { get; set; }
        [Ignore]
        public string InfantryTag { get; set; }
    }

    static class Program
    {
        static void Main(string[] args)
        {

            //Grab all the records from the files
            var filesToRead = Directory.GetFiles("data", "*.csv");
            var ingestList = Enumerable.Empty<CsvRecord>();
            foreach (var file in filesToRead)
            {
                using (var sr = new StreamReader(file))
                {
                    using (var reader = new CsvReader(sr, CultureInfo.InvariantCulture))
                    {
                        ingestList = ingestList.Union(reader.GetRecords<CsvRecord>().ToList());
                    }
                }
            }

            //Perform some corrections
            foreach (var item in ingestList)
            {
                
                switch (item.WeaponName)
                {
                    //Flash, Bastion, Colossus weapons and construction/turrets/orbitals not tagged correctly
                    case "LA7 Buzzard":
                    case "M40 Fury-F":
                    case "M4-F Pillager":
                    case "M20 Basilisk-F":
                    case "V30-F Starfall":
                        item.Vehicle = "Flash";
                        break;
                    case "Mammoth Cannon":
                    case "M20 Gecko":
                    case "M60 Pug":
                    case "M75 Fang":
                    case "Dingo ML-6":
                        item.Vehicle = "Colossus";
                        break;
                    case "Bastion CIWS"://Not really vehicles but this will do to take them out of the infantry filter
                    case "Bastion Battery":
                    case "Bastion CDWS":
                    case "Mauler Cannon":
                        item.Vehicle = "Bastion";
                        item.NotVehicle = true;
                        break;
                    case "Hoplon Anti-Aircraft Phalanx Tower":
                    case "Spear Anti-Vehicle Phalanx Tower":
                    case "Xiphos Anti-Personnel Phalanx Tower":
                    case "Xiphos Anti-Personnel Phalanx Turret":
                    case "Spear Anti-Vehicle Phalanx Turret":
                    case "Aspis Anti-Aircraft Phalanx Turret":
                    case "Aspis Anti-Aircraft Phalanx Turret "://wat
                        item.Vehicle = "Base Turret";
                        item.NotVehicle = true;
                        break;
                    case "Glaive IPC":
                    case "The Flail":
                        item.Vehicle = "Construction Artillery";
                        item.NotVehicle = true;
                        break;
                    case "Armory War Asset Orbital Strike":
                        item.Vehicle = "Pocket Orbital";
                        item.NotVehicle = true;
                        break;
                    case "Orbital Strike Uplink":
                        item.Vehicle = "Construction Orbital";
                        item.NotVehicle = true;
                        break;
                    case "Suicide":
                        item.Vehicle = "Suicide";
                        item.NotVehicle = true;
                        break;

                    //Apply tags to infantry stuff
                    case "NS Decimator":
                    case "NS \"Heatwave\" Decimator":
                    case "NS \"Jackpot\" Decimator":
                    case "NS Decimator-B":
                    case "NS Decimator-G":
                    case "Shrike":
                    case "ML-7":
                    case "S1":
                    case "NSX Masamune":
                    case "NS Annihilator":
                    case "NS Annihilator-B":
                    case "NS Annihilator-G":
                    case "T2 Striker":
                    case "Striker AE":
                    case "Nemesis VSH9":
                    case "M9 SKEP Launcher":
                    case "ASP-30 Grounder":
                    case "AF-22 Crow":
                    case "Hawk GD-68":
                    case "NC15 Phoenix":
                    case "Phoenix AE":
                    case "Hades VSH4":
                    case "Lancer AE":
                    case "Lancer VS22":
                    case "NS-R3 Swarm":
                    case "NS-R3 \"Ravenous\" Swarm":
                    case "NSX-A Muramasa":
                    case "The Kraken":
                        item.InfantryTag = "Rocket Launcher";
                        break;
                    case "C-4":
                    case "C-4 ARX":
                        item.InfantryTag = "C4";
                        break;
                    case "Hunter QCX":
                    case "Hunter QCX-B":
                    case "Hunter QCX-G":
                    case "Heartstring":
                    case "Blackheart":
                    case "Hunter QCX-P":
                        item.InfantryTag = "Crossbow";
                        break;
                    case "Grenade Printer":
                    case "M3 Pounder HEG":
                    case "NCM2 Falcon":
                    case "Comet VM2":
                    case "MR1 Fracture":
                    case "NCM3 Raven":
                    case "Vortex VM21":
                    case "NS-20 Gorgon":
                    case "NS-20G Gorgon":
                    case "NS-20B Gorgon":
                    case "NS-20 \"Heatwave\" Gorgon":
                    case "NS-10 Burster":
                    case "NS-10B Burster":
                    case "NS-10G Burster":
                        item.InfantryTag = "MAX";
                        break;
                }


                if (item.AttackerWeaponId == 6010049) //API bruh moment
                {
                    item.WeaponName = "NS Scorpion";
                    item.InfantryTag = "Rocket Launcher";
                }

                if (item.AttackerWeaponId == 0)//Even bigger API bruh moment
                {
                    item.WeaponName = "Friggin Magic";
                    item.NotVehicle = true;
                }
            }

            var groupedByVehicle = ingestList.Where(x => !(string.IsNullOrEmpty(x.Vehicle) || x.NotVehicle)).GroupBy(x=>x.Vehicle);
            var infantryKills = ingestList.Where(x => string.IsNullOrEmpty(x.Vehicle) && x.AttackerWeaponId!=0);
            Console.WriteLine("Infantry kills total: {0}", infantryKills.Sum(x => x.NumKills));

            Console.WriteLine("Rocket Launchers: {0}", infantryKills.Where(x => x.InfantryTag == "Rocket Launcher").Sum(x=>x.NumKills));
            Console.WriteLine("C-4: {0}", infantryKills.Where(x => x.InfantryTag == "C4").Sum(x => x.NumKills));
            Console.WriteLine("Rocklet Rifle: {0}", infantryKills.Where(x => x.WeaponName == "Rocklet Rifle").Sum(x => x.NumKills));
            Console.WriteLine("Tank Mines: {0}", infantryKills.Where(x => x.WeaponName == "Tank Mine").Sum(x => x.NumKills));
            Console.WriteLine("Crossbows: {0}", infantryKills.Where(x => x.InfantryTag == "Crossbow").Sum(x => x.NumKills));
            Console.WriteLine("MAX weapons: {0}", infantryKills.Where(x => x.InfantryTag == "MAX").Sum(x => x.NumKills));
            Console.WriteLine("Infantry weapons (grouped by name)**********");
            var infantryByName = ingestList.Where(x => string.IsNullOrEmpty(x.Vehicle) && x.AttackerWeaponId != 0).GroupBy(x=>x.WeaponName).OrderByDescending(x=>x.Sum(x=>x.NumKills));
            foreach (var item in infantryByName.OrderByDescending(x => x.Sum(y => y.NumKills)))
            {
                Console.WriteLine("{0} {1} [{2}]", item.Key, item.Sum(x => x.NumKills), item.FirstOrDefault().InfantryTag);
            }

            Console.WriteLine();
            Console.WriteLine("Vehicle kills: {0}", groupedByVehicle.Sum(x=>x.Sum(y=>y.NumKills)));
            Console.WriteLine("Vehicle kills (grouped by vehicle)**********");
            foreach(var item in groupedByVehicle.OrderByDescending(x => x.Sum(y => y.NumKills)))
            {
                Console.WriteLine("{0} {1}", item.Key, item.Sum(x => x.NumKills));
            }

            Console.WriteLine();
            Console.WriteLine("Bastion Fleet Carrier kills (all weapons): {0}", ingestList.Where(x => x.Vehicle == "Bastion").Sum(x => x.NumKills));
            Console.WriteLine("Pocket Orbitals: {0}", ingestList.Where(x => x.Vehicle == "Pocket Orbital").Sum(x => x.NumKills));
            Console.WriteLine("Construction Orbitals: {0}", ingestList.Where(x => x.Vehicle == "Construction Orbital").Sum(x => x.NumKills));
            Console.WriteLine("Various base/construction turrets: {0}", ingestList.Where(x => x.Vehicle == "Base Turret").Sum(x => x.NumKills));
            Console.WriteLine("Suicide: {0}", ingestList.Where(x => x.Vehicle == "Suicide").Sum(x => x.NumKills));
            Console.WriteLine("Friggin Magic (weapon id 0): {0}", ingestList.Where(x => x.AttackerWeaponId==0).Sum(x => x.NumKills));
        }
    }
}
