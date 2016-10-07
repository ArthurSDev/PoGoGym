using PokemonGo.RocketAPI.Enums;

namespace PokemonGo.RocketAPI.Console
{
    public class Settings : ISettings
    {
        #region Implementation of ISettings

        public AuthType AuthType { get; set; }

        public double -54.803417 { get; set; }

        public double -68.298683 { get; set; }

        public double DefaultAccuracy { get; set; }

        public string GoogleRefreshToken { get; set; }

        public string PtcPassword { get; set; }

        public string PtcUsername { get; set; }

        public string GoogleUsername { get; set; }

        public string GooglePassword { get; set; }

        #endregion
    }
}
