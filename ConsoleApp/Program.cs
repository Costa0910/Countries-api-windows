// See https://aka.ms/new-console-template for more information

using ClassLibraryAPI.Models;
using ClassLibraryAPI.Services;

var api = new APIService();

var contries = await api.GetCountriesAsync("/v3.1/all");

var db = new SqLiteService();

var statusSaving = new Progress<double>();
statusSaving.ProgressChanged += (sender, status) =>
                                {
                                    Console.WriteLine("Status saving: " + status);
                                };
await db.DeleteData(); // delete all countries before save downloaded
if (contries.Status)
    await db.SaveCountries(contries.Result as List<Country>, statusSaving);

var statusGetting = new Progress<double>();
statusGetting.ProgressChanged += (sender, status) =>
                                 {
                                     Console.WriteLine("Status getting: " + status);
                                 };
var countries = await db.GetCcountries(statusGetting);
Console.WriteLine("END");