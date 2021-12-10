using BloombergAPI;
using Microsoft.Extensions.Configuration;

var cfg = new ConfigurationManager();

cfg["Vendors:Bloomberg:Host"] = "localhost";
cfg["Vendors:Bloomberg:Port"] = "8194";

using var bloom = new BloombergAPI.BloombergAPI(cfg);
var req = bloom.CurrentRequest(new[] {"AAPL UW Equity"}, new[] {"COMPANY_ADDRESS", "PX_LAST"});

foreach (var (_, value) in req.Data)
{
    foreach (var (_, output) in value)
    {
        if (output.IsListOfObjects())
        {
            foreach (var item in output.AsListOfObjects())
            {
                Console.WriteLine(item.ToString());
            }
        }
        else
        {
            Console.WriteLine(output.ToString());
        }
    }
}