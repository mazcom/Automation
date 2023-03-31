// See https://aka.ms/new-console-template for more information
using ProducerConsumerConsoleApp.Models;

Console.WriteLine("Hello, World!");


var environmets = new List<EnvironmentModel>()
{
  new EnvironmentModel("Env1") {
    Tests = new List<TestModel>
      {
        new TestModel("SC1"),
        new TestModel("SC2"),
        new TestModel("SC3")
      }},
  new EnvironmentModel("Env2") {
    Tests = new List<TestModel>
      {
        new TestModel("Datagenerator1"),
        new TestModel("Datagenerator2")
      }},
  new EnvironmentModel("Env3") {
    Tests = new List<TestModel>
      {
        new TestModel("Import1")
      }},
};


