using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography.X509Certificates;
using System.Linq.Expressions;
namespace SmartDeliverySystem
{
    public struct DeliveryAddress
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public DeliveryAddress(string street, string city, string zipcode)
        {
            Street = street;
            City = city;
            ZipCode = ZipCode;
        }
        public override string ToString()
        {
            return $"street:{Street}, city:{City}, zipcode:{ZipCode}";
        }
    }
    public interface ITrackable
    {
        string GetTrackingStatus();
    }
    public interface IInsurable
    {
        decimal CalculateInsurance();
    }
    public abstract partial class Shipment : ITrackable, IInsurable
    {
        private string _trackingCode;
        private string _description;
        private decimal _weight;
        private decimal _deliveryFee;
        public DeliveryAddress Destination { get; set; }

        public string TrackingCode
        {
            get { return _trackingCode; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    _trackingCode = value;
                }
                else
                {
                    throw new ArgumentException("Tracking code cannot be null or empty.");
                }
            }
        }

        public string Description
        {
            get { return _description; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    _description = value;
                }
                else
                {
                    throw new ArgumentException("Description cannot be null or empty.");
                }
            }
        }

        public decimal Weight
        {
            get { return _weight; }
            set
            {
                if (value > 0)
                {
                    _weight = value;
                }
                else
                {
                    throw new ArgumentException("Weight must be greater than zero.");
                }
            }
        }

        public decimal DeliveryFee
        {
            get { return _deliveryFee; }
            set
            {
                if (value >= 0)
                {
                    _deliveryFee = value;
                }
                else
                {
                    throw new ArgumentException("Delivery fee cannot be negative.");
                }
            }
        }

        public abstract decimal EstimatedCost();
        public abstract void PrintShipment();

        public abstract string GetTrackingStatus();
        public abstract decimal CalculateInsurance();

        public static int TotalShipmentsCreated;
        static Shipment()
        {
            TotalShipmentsCreated = 0;
            Console.WriteLine("Shipment System Initialized");
        }
        public static int GetTotalShipmentsCreated() => TotalShipmentsCreated;



        public Shipment(string trackingcode, string description, decimal weight, decimal deliveryFee, DeliveryAddress destination)
        {
            TrackingCode = trackingcode;
            Description = description;
            Weight = weight;
            DeliveryFee = deliveryFee;
            Destination = destination;
            TrackingStatus = "Ready";

            TotalShipmentsCreated++;
        }

        public void UpdateWeight(decimal newWeight)
        {
            Weight = newWeight;
        }

        public void UpdateWeight(decimal newWeight, decimal extraPackingWeight)
        {
            Weight = newWeight + extraPackingWeight;
        }

        public abstract Shipment DeepCopy();
        public Shipment ShallowCopy()
        {
            return (Shipment)this.MemberwiseClone();
        }


    }
    public abstract partial class Shipment
    {
        private string _trackingStatus;

        public string TrackingStatus
        {
            get
            {
                return _trackingStatus;
            }
            protected set
            {
                _trackingStatus = value;

            }
        }

        public void UpdateTrackingStatus(string newStatus)
        {
            TrackingStatus = newStatus;
        }
    }

    public class StandardShipment : Shipment
    {
        public StandardShipment(string trackingcode, string description, decimal weight, decimal deliveryFee, DeliveryAddress destination)
            : base(trackingcode, description, weight, deliveryFee, destination)
        {
        }
        public override void PrintShipment()
        {
            Console.WriteLine("======== STANDARD SHIPMENT ========");
            Console.WriteLine($"Tracking Code: {TrackingCode}");
            Console.WriteLine($"description: {Description}");
            Console.WriteLine($"Weight: {Weight} Kg");
            Console.WriteLine($"DeliveryFee: {DeliveryFee} LE");
            Console.WriteLine($"Estimated Cost: {EstimatedCost()}");

        }
        public override decimal EstimatedCost()
        {
            return DeliveryFee + (Weight * 5);
        }
        public override string GetTrackingStatus()
        {
            return $"Shipment {TrackingCode} is {TrackingStatus}";
        }

        public override decimal CalculateInsurance()
        {
            return EstimatedCost() * 0.05m;
        }
        public override Shipment DeepCopy()
        {
            var copy = new StandardShipment(TrackingCode, Description, Weight, DeliveryFee, new DeliveryAddress(Destination.Street, Destination.City, Destination.ZipCode));
            copy.TrackingStatus = this.TrackingStatus;
            return copy;
        }

    }

    public class ExpressShipment : Shipment
    {
        private decimal _extraFee;

        public decimal ExtraFee
        {
            get => _extraFee;
            set
            {
                if (value < 0) throw new ArgumentException("Extra fee cannot be negative.");
                _extraFee = value;
            }
        }

        public ExpressShipment(string trackingcode, string description, decimal weight, decimal deliveryFee, DeliveryAddress destination, decimal extrafee)
            : base(trackingcode, description, weight, deliveryFee, destination)
        {
            ExtraFee = extrafee;
        }
        public override void PrintShipment()
        {
            Console.WriteLine("======== EXPRESS SHIPMENT ========");
            Console.WriteLine($"Tracking Code: {TrackingCode}");
            Console.WriteLine($"description: {Description}");
            Console.WriteLine($"Weight: {Weight} Kg");
            Console.WriteLine($"DeliveryFee: {DeliveryFee} LE");
            Console.WriteLine($"Estimated Cost: {EstimatedCost()}");
            Console.WriteLine($"Extra Fee : {ExtraFee} LE");
        }

        public override decimal EstimatedCost()
        {
            return DeliveryFee + Weight * 5 + ExtraFee;
        }
        public override string GetTrackingStatus()
        {
            return $"Shipment {TrackingCode} is {TrackingStatus}";
        }
        public override decimal CalculateInsurance()
        {
            return EstimatedCost() * 0.08m;
        }
        public override Shipment DeepCopy()
        {
            var copy = new ExpressShipment(TrackingCode, Description, Weight, DeliveryFee, new DeliveryAddress(Destination.Street, Destination.City,
                Destination.ZipCode), ExtraFee);
            copy.TrackingStatus = this.TrackingStatus;
            return copy;
        }
    }
    public class InternationalShipment : Shipment
    {
        private decimal _customsFee;
        private string _destinationCountry;
        public decimal CustomsFee
        {
            get { return _customsFee; }
            set
            {
                if (value < 0) throw new ArgumentException("Customs fee cannot be negative.");
                _customsFee = value;
            }
        }
        public string DestinationCountry
        {
            get { return _destinationCountry; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    _destinationCountry = value;
                }
                else
                {
                    throw new ArgumentException("Destination country cannot be null or empty.");
                }
            }
        }


        public InternationalShipment(string trackingcode, string description, decimal weight, decimal deliveryFee, DeliveryAddress destination, string destinationCountry, decimal customsFee)
            : base(trackingcode, description, weight, deliveryFee, destination)
        {
            DestinationCountry = destinationCountry;
            CustomsFee = customsFee;
        }
        public override void PrintShipment()
        {
            Console.WriteLine("======== INTERNATIONAL SHIPMENT ========");
            Console.WriteLine($"Tracking Code: {TrackingCode}");
            Console.WriteLine($"description: {Description}");
            Console.WriteLine($"Weight: {Weight} Kg");
            Console.WriteLine($"DeliveryFee: {DeliveryFee} LE");
            Console.WriteLine($"Estimated Cost: {EstimatedCost()}");
            Console.WriteLine($"Destination Country: {DestinationCountry}");
            Console.WriteLine($"Customs Fee: {CustomsFee} LE");
        }
        public override decimal EstimatedCost()
        {
            return DeliveryFee + Weight * 5 + CustomsFee;
        }
        public override string GetTrackingStatus()
        {
            return $"Shipment {TrackingCode} is {TrackingStatus}";
        }
        public override decimal CalculateInsurance()
        {
            return EstimatedCost() * 0.12m;
        }
        public virtual void GenerateCustomReport()
        {
            Console.WriteLine($"Customs report generated for shipment {TrackingCode} going to {DestinationCountry}");

        }
        public override Shipment DeepCopy()
        {
            var copy = new InternationalShipment(TrackingCode, Description, Weight, DeliveryFee, new DeliveryAddress(Destination.Street, Destination.City,
                Destination.ZipCode), DestinationCountry, CustomsFee);
            copy.TrackingStatus = this.TrackingStatus;
            return copy;
        }
    }
    public class PriorityInternationalShipment : InternationalShipment
    {
        public PriorityInternationalShipment(string trackingCode, string description, decimal weight, decimal deliveryFee, DeliveryAddress destination, string destinationCountry, decimal customsFee)
             : base(trackingCode, description, weight, deliveryFee, destination, destinationCountry, customsFee) { }

        public sealed override void GenerateCustomReport()
        {
            Console.WriteLine($"Priority Express Customs Clearance Completed for {TrackingCode}.");
        }
    }

    public sealed class CompletedShipment : StandardShipment
    {
        public CompletedShipment(string trackingcode, string description, decimal weight, decimal deliveryFee, DeliveryAddress destination)
            : base(trackingcode, description, weight, deliveryFee, destination) { }
    }

    public class DeliveryCenter
    {
        public String CenterName { get; set; }
        public Shipment[] Shipments = new Shipment[100];
        public int _Counter = 0;  //number of shipments currently in the center
        public DeliveryCenter(string centerName)
        {
            CenterName = centerName;
        }
        public Shipment this[int index]
        {

           
            get
            {
                if (index < 0 || index >= _Counter)
                {
                    throw new IndexOutOfRangeException("Index is out of range.");
                }
                return Shipments[index];
            }
            set
            {
                if (index < 0 || index >= _Counter)
                {
                    throw new IndexOutOfRangeException("Index is out of range.");
                }
                Shipments[index] = value;
            }
        }
        public Shipment this[string trackingCode]
        {
            get
            {
                for (int i = 0; i < _Counter; i++)
                {
                    if (Shipments[i].TrackingCode == trackingCode)
                    {
                        return Shipments[i];
                    }
                }
                throw new KeyNotFoundException($"Shipment with tracking code {trackingCode} not found.");
            }
        }

        public bool AddShipment(Shipment newshipment)
        {
            if (_Counter >= Shipments.Length || newshipment == null)
                return false;

            Shipments[_Counter] = newshipment;
            _Counter++;

            return true;
        }

        public void RemoveShipment(string trackingCode)
        {
            for (int i = 0; i < _Counter; i++)
            {
                if (Shipments[i].TrackingCode == trackingCode)
                {
                    for (int j = i; j < _Counter - 1; j++)
                    {
                        Shipments[j] = Shipments[j + 1];
                    }
                    Shipments[--_Counter] = null;
                    Console.WriteLine($"Shipment with tracking code {trackingCode} removed successfully.");
                }
            }
            throw new KeyNotFoundException($"Shipment with tracking code {trackingCode} not found.");
        }

        public void PrintAllShipments()
        {
            for (int i = 0; i < _Counter; i++)
            {
                Shipments[i].PrintShipment();
                Console.WriteLine("------------------------------------------");
            }

        }
        public void PrintTrackingStatuses()
        {
            for (int i = 0; i < _Counter; i++)
            {
                Console.WriteLine(Shipments[i].GetTrackingStatus());
            }
        }

    }
    public static class DeliveryUtillities
    {
        public static void PrintSepertaor()
        {
            Console.WriteLine("===================================");
        }

        public static void PrintSystemTitle(string title)
        {
            PrintSepertaor();
            Console.WriteLine(title);
            PrintSepertaor();
        }
    }
    public static class DeliveryHelper
    {
        public static void PrintShipmentDetails(Shipment shipment)
        {
            shipment.PrintShipment();

        }

    }
    public static class DeliveryReport
    {
        public static void PrintShipment(ITrackable shipment) => Console.WriteLine(shipment.GetTrackingStatus());
        public static void PrintInsurance(IInsurable shipment) => Console.WriteLine($"Insurance Cost: {shipment.CalculateInsurance():0.00} EGP");
    }




    //==================================== PROGRAM ============================================




    internal class Program
    {
        static void Main(string[] args)
        {
            DeliveryUtillities.PrintSystemTitle("Smart Delivery System");

            var address1 = new DeliveryAddress("123 St", "Cairo", "11511");
            var address2 = new DeliveryAddress("456 Ave", "Giza", "12511");
            var address3 = new DeliveryAddress("789 Blvd", "Alexandria", "21511");


            DeliveryUtillities.PrintSystemTitle("Creating Shipments");

            var sh1 = new StandardShipment(
                "SH001",
                "Laptop",
                3m,
                80m,
                address1
            );

            var sh2 = new ExpressShipment(
                "SH002",
                "Mobile Phone",
                2m,
                60m,
                address2,
                30m
            );

            var sh3 = new InternationalShipment(
                "SH003",
                "Television",
                8m,
                120m,
                address3,
                "Germany",
                100m
            );


           
            sh2.UpdateTrackingStatus("Out For Delivery");
            sh3.UpdateTrackingStatus("Delivered");

            Console.WriteLine(
                $"\nTotal Shipments Created : {Shipment.GetTotalShipmentsCreated()}"
            );


            DeliveryUtillities.PrintSystemTitle("Delivery Center Inventory");

            var center = new DeliveryCenter("Cairo Central Hub");

            center.AddShipment(sh1);
            center.AddShipment(sh2);
            center.AddShipment(sh3);

            center.PrintAllShipments();


            DeliveryUtillities.PrintSystemTitle("Interface Polymorphism");

            ITrackable[] trackableShipments =
            {
    sh1,
    sh2,
    sh3
};

            IInsurable[] insurableShipments =
            {
    sh1,
    sh2,
    sh3
};


            Console.WriteLine("Tracking Statuses:");

            foreach (var t in trackableShipments)
            {
                DeliveryReport.PrintShipment(t);
            }


            Console.WriteLine("\nInsurance Values:");

            foreach (var i in insurableShipments)
            {
                DeliveryReport.PrintInsurance(i);
            }


            DeliveryUtillities.PrintSystemTitle("Object Copying Demonstration");

            
            Shipment assigned = sh1;

            Console.WriteLine(
                $"Same Reference Object Assignment: {ReferenceEquals(sh1, assigned)}"
            );


            
            Shipment shallowCloned = sh1.ShallowCopy();

            
            Shipment deepCloned = sh1.DeepCopy();

            Console.WriteLine(
                $"Shallow Copy is New Object : {!ReferenceEquals(sh1, shallowCloned)}"
            );

            Console.WriteLine(
                $"Deep Copy is New Object    : {!ReferenceEquals(sh1, deepCloned)}"
            );


            DeliveryUtillities.PrintSystemTitle(
                "Assignment Completed Successfully"
            );

        }
    }
}




























