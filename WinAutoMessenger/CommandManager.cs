using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibreHardwareMonitor;

namespace WinAutoMessenger
{
    public class CommandManager
    {
        private readonly static LibreHardwareMonitor.Hardware.Computer m_comp = new LibreHardwareMonitor.Hardware.Computer()
        {
            IsControllerEnabled = false,
            IsCpuEnabled = true,
            IsGpuEnabled = false,
            IsMemoryEnabled = false,
            IsMotherboardEnabled = false,
            IsNetworkEnabled = false,
            IsStorageEnabled = true,
        };

        private string get_full_report()
        {
            try
            {
                m_comp.Open();
                return m_comp.GetReport();
            }
            finally
            {
                m_comp.Close();
            }
        }

        public struct Sensor<T>
        {
            public readonly int Index;
            public readonly string Name;
            public readonly T Value;

            public Sensor(int index, string name, T value)
            {
                this.Index = index;
                this.Name = name;
                this.Value = value;
            }
        }
        public struct CpuInfo
        {
            public string UniqueId;
            public string Identityfier;
            public int NumOfCores;
            public List<Sensor<float>> MaxFrequencies;

            public List<Sensor<float>> CpuUsagePercentage;
        }
        public static CpuInfo? get_cpu_info()
        {
            try
            {
                m_comp.Open();
                CpuInfo info = new CpuInfo();
                foreach (var h in m_comp.Hardware)
                {
                    string report = h.GetReport();
                    
                    if (h.HardwareType == LibreHardwareMonitor.Hardware.HardwareType.Cpu)
                    {
                        info.UniqueId = h.Name;
                        info.Identityfier = h.Identifier.ToString();
                        foreach (var s in h.Sensors)
                        {
                            if (s.SensorType == LibreHardwareMonitor.Hardware.SensorType.Clock)
                            {
                                if (info.MaxFrequencies == null)
                                    info.MaxFrequencies = new List<Sensor<float>>();
                                
                                if(s.Max.HasValue)
                                {
                                    info.MaxFrequencies.Add(new Sensor<float>(s.Index, s.Name, s.Max.Value));
                                }
                            }

                            if(s.SensorType == LibreHardwareMonitor.Hardware.SensorType.Load)
                            {
                                if (info.CpuUsagePercentage == null)
                                    info.CpuUsagePercentage = new List<Sensor<float>>();

                                if (s.Value.HasValue)
                                    info.CpuUsagePercentage.Add(new Sensor<float>(s.Index, s.Name, s.Value.Value));

                                info.NumOfCores = info.CpuUsagePercentage.Count - 1;
                            }
                        }
                    }
                }
                return string.IsNullOrWhiteSpace(info.UniqueId) ? null : (CpuInfo?)info;
            }
            finally
            {
                m_comp.Close();
            }
        }
        public enum StorageType
        {
            HDD,
            SSD,
            HYBRID,
            EXTERNAL,
        };

        public struct PartitionInfo
        {
            public readonly string Name;
            public readonly string Volume;

            public readonly long Capacity;
            public readonly long UsedCapacity;
            public readonly long FreeCapacity;

            public PartitionInfo(string name, string volume, long capacity, long usedcapacity, long freecapacity)
            {
                this.Name = name;
                this.Volume = volume;
                this.Capacity = capacity;
                this.UsedCapacity = usedcapacity;
                this.FreeCapacity = freecapacity;
            }
        }
        public struct StorageInfo
        {
            public string Id;
            public string Name;

            public List<PartitionInfo> Partitions;
            //public StorageType Type;
        }

        public static StorageInfo? get_storage_info()
        {
            try
            {
                m_comp.Open();
                StorageInfo info = new StorageInfo();
                foreach (var h in m_comp.Hardware)
                {
                    string report = h.GetReport();

                    if (h.HardwareType == LibreHardwareMonitor.Hardware.HardwareType.Storage &&
                        h is LibreHardwareMonitor.Hardware.Storage.GenericHardDisk)
                    {
                        LibreHardwareMonitor.Hardware.Storage.GenericHardDisk ghd = h as LibreHardwareMonitor.Hardware.Storage.GenericHardDisk;

                        info.Id = ghd.Identifier.ToString();
                        info.Name = ghd.Name;

                        foreach (var dir in ghd.DriveInfos)
                        {
                            if(info.Partitions == null)
                                info.Partitions = new List<PartitionInfo>();


                            info.Partitions.Add(new PartitionInfo(dir.Name, dir.VolumeLabel, dir.TotalSize, dir.TotalSize - dir.TotalFreeSpace, dir.TotalFreeSpace));
                        }
                    }
                }
                return (StorageInfo?)info;
            }
            finally
            {
                m_comp.Close();
            }
        }

        public static string to_json(StorageInfo info)
        {
            JSONWriter j = new JSONWriter();
            j.Begin();
            j.AddPair("id", info.Id);
            j.AddPair("name", info.Name);
            if(info.Partitions?.Count > 0)
            {
                j.BeginStructureArray("partitions");
                foreach (PartitionInfo pinf in info.Partitions)
                {
                    j.BeginStructureArrayElement();

                    j.AddStructureArrayPair("name", pinf.Name);
                    j.AddStructureArrayPair("volume", pinf.Volume);

                    j.AddStructureArrayPair("capacity", pinf.Capacity);
                    j.AddStructureArrayPair("usedcapacity", pinf.UsedCapacity);
                    j.AddStructureArrayPair("freecapacity", pinf.FreeCapacity);

                    j.EndStructureArrayElement();
                }
                j.EndStructureArray();
            }
            
            j.End();
            return j.Generate();
        }
        public static string to_json(CpuInfo info)
        {
            JSONWriter j = new JSONWriter();
            j.Begin();
            j.AddPair("id", info.UniqueId);
            j.AddPair("identityfier", info.Identityfier);
            j.AddPair("cores", info.NumOfCores);
            if (info.MaxFrequencies?.Count > 0)
            {
                j.BeginStructureArray("freq");
                foreach (var freq in info.MaxFrequencies)
                {
                    j.BeginStructureArrayElement();

                    j.AddStructureArrayPair("index", freq.Index);
                    j.AddStructureArrayPair("name", freq.Name);
                    j.AddStructureArrayPair("max", freq.Value);

                    j.EndStructureArrayElement();
                }
                j.EndStructureArray();
            }
            if (info.MaxFrequencies?.Count > 0)
            {
                j.BeginStructureArray("usage");
                foreach (var usg in info.CpuUsagePercentage)
                {
                    j.BeginStructureArrayElement();

                    j.AddStructureArrayPair("index", usg.Index);
                    j.AddStructureArrayPair("name", usg.Name);
                    j.AddStructureArrayPair("value", usg.Value);

                    j.EndStructureArrayElement();
                }
                j.EndStructureArray();
            }

            j.End();
            return j.Generate();
        }





        public static string RunCommand(string command)
        {
            if (command == null)
                return null;
            command = command.Replace("\n", "");
            command = command.Replace("\r", "");
            command = command.ToLower();
            command = command.Trim();


            
            if(command == "report cpu")
            {
                var stg = get_cpu_info();
                return stg.HasValue ? to_json(stg.Value) : "Failed...";
            }
            if (command == "report storage")
            {
                var stg = get_storage_info();
                return stg.HasValue ? to_json(stg.Value) : "Failed...";

            }
            return $"cannot recognize the command '{command}'";
        }
    }
}
