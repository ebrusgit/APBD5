using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using AnimalDatabase.Models;
using AnimalDatabase.Models.DTOs;

namespace AnimalDatabase.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnimalsController : ControllerBase
{
    private readonly IConfiguration _configuration;
    public AnimalsController(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    [HttpGet]
    public IActionResult GetAnimals([FromQuery] string orderBy = "name")
    {
        string query = orderBy.ToLower() switch
        {
            "name" => "SELECT * FROM Animal ORDER BY Name ASC",
            "description" => "SELECT * FROM Animal ORDER BY Description ASC",
            "category" => "SELECT * FROM Animal ORDER BY Category ASC",
            "area" => "SELECT * FROM Animal ORDER BY Area ASC",
            _ => "SELECT * FROM Animal ORDER BY Name ASC",
        };

        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        connection.Open();
        using SqlCommand command = new SqlCommand(query, connection);
        var reader = command.ExecuteReader();

        var animals = new List<Animal>();
        while (reader.Read())
        {
            animals.Add(new Animal()
            {
                IdAnimal = reader.GetInt32(reader.GetOrdinal("IdAnimal")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
            });
        }
        return Ok(animals);
    }

    [HttpPost]
    public IActionResult AddAnimal([FromBody] AddAnimal animal)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        connection.Open();
        using SqlCommand command = new SqlCommand("INSERT INTO Animal VALUES (@animalName,'','','')", connection);
        command.Parameters.AddWithValue("@animalName", animal.Name);

        command.ExecuteNonQuery();

        return Created("", null);
    }

    [HttpPut("{idAnimal}")]
    public IActionResult UpdateAnimal(int idAnimal, [FromBody] UpdateAnimalDto updateAnimalDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        connection.Open();
        string commandText = "UPDATE Animal SET Name = @Name WHERE IdAnimal = @IdAnimal";
        using SqlCommand command = new SqlCommand(commandText, connection);
        command.Parameters.AddWithValue("@IdAnimal", idAnimal);
        command.Parameters.AddWithValue("@Name", updateAnimalDto.Name);
        int affectedRows = command.ExecuteNonQuery();

        if (affectedRows == 0)
            return NotFound();
        return NoContent();
    }

    [HttpDelete("{idAnimal}")]
    public IActionResult DeleteAnimal(int idAnimal)
    {
        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        connection.Open();
        string commandText = "DELETE FROM Animal WHERE IdAnimal = @IdAnimal";
        using SqlCommand command = new SqlCommand(commandText, connection);
        command.Parameters.AddWithValue("@IdAnimal", idAnimal);
        int affectedRows = command.ExecuteNonQuery();

        if (affectedRows == 0)
            return NotFound();
        return NoContent();
    }
}
