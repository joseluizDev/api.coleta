using System.Collections.Generic;

namespace api.coleta.models
{
    public class ColetasRequestDTO
    {
        public string Nome { get; set; }
        public string Endereco { get; set; }
        public Location location { get; set; }
        public string reference { get; set; }

        public string Place_Id { get; set; }
    }

    public class ColetasResponseDTO
    {
        public Guid Id { get; set; }
        public string Nome { get; set; }
        public string Endereco { get; set; }
        public Location location { get; set; }
        public string reference { get; set; }
        public string Place_Id { get; set; }
    }

    public class AddressComponent
    {
        public string LongName { get; set; }
        public string ShortName { get; set; }
        public List<string> Types { get; set; }
    }

    public class Location
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }

    public class Northeast
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }

    public class Southwest
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }

    public class Bounds
    {
        public Northeast Northeast { get; set; }
        public Southwest Southwest { get; set; }
    }

    public class Viewport
    {
        public Northeast Northeast { get; set; }
        public Southwest Southwest { get; set; }
    }

    public class Geometry
    {
        public Bounds Bounds { get; set; }
        public Location Location { get; set; }
        public Viewport Viewport { get; set; }
    }

    public class OpeningHours
    {
        public bool OpenNow { get; set; }
    }

    public class Photo
    {
        public int Height { get; set; }
        public List<string> HtmlAttributions { get; set; }
        public string PhotoReference { get; set; }
        public int Width { get; set; }
    }

    public class PlusCode
    {
        public string CompoundCode { get; set; }
        public string GlobalCode { get; set; }
    }

    public class Result
    {
        public Geometry Geometry { get; set; }
        public string Name { get; set; }
        public string Vicinity { get; set; }
        public string BusinessStatus { get; set; }
        public string Icon { get; set; }
        public string IconBackgroundColor { get; set; }
        public string IconMaskBaseUri { get; set; }
        public OpeningHours OpeningHours { get; set; }
        public List<Photo> Photos { get; set; }
        public string Place_Id { get; set; }
        public PlusCode PlusCode { get; set; }
        public double Rating { get; set; }
        public string Reference { get; set; }
        public string Scope { get; set; }
        public List<string> Types { get; set; }
        public int UserRatingsTotal { get; set; }
    }

    public class Root
    {
        public List<object> HtmlAttributions { get; set; }
        public List<Result> Results { get; set; }
        public string Status { get; set; }
    }
}
