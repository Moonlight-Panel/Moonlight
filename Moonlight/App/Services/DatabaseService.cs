using aaPanelSharp;
using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database.Entities;
using Moonlight.App.Exceptions;
using Moonlight.App.Repositories;

namespace Moonlight.App.Services;

public class DatabaseService
{
    private readonly DatabaseRepository DatabaseRepository;
    private readonly AaPanelRepository AaPanelRepository;

    public DatabaseService(DatabaseRepository databaseRepository, AaPanelRepository aaPanelRepository)
    {
        DatabaseRepository = databaseRepository;
        AaPanelRepository = aaPanelRepository;
    }

    public Task<Database.Entities.Database> Create(string name, string password, User u, AaPanel? a = null)
    {
        if (DatabaseRepository.Get().Any(x => x.Name == name))
            throw new DisplayException("A database with this name has been already created");
        
        var aaPanel = a ?? AaPanelRepository.Get().First();

        var access = new aaPanel(a!.Url, a.Key);

        if (access.CreateDatabase(name, name, password))
        {
            var aaDb = access.Databases.First(x => x.Name == name);

            return Task.FromResult(DatabaseRepository.Add(new()
            {
                Name = name,
                AaPanel = aaPanel,
                Owner = u,
                InternalAaPanelId = aaDb.Id
            }));
        }
        else
            throw new DisplayException("An unknown error occured while creating the database");
    }

    public Task ChangePassword(Database.Entities.Database database, string newPassword)
    {
        var access = CreateApiAccess(database);

        access.Databases.First(x => x.Id == database.InternalAaPanelId).ChangePassword(newPassword);
        
        return Task.CompletedTask;
    }

    public Task<string> GetPassword(Database.Entities.Database database)
    {
        var access = CreateApiAccess(database);

        return Task.FromResult(
            access.Databases
                .First(x => x.Id == database.InternalAaPanelId).Password
        );
    }
    
    private aaPanel CreateApiAccess(Database.Entities.Database database)
    {
        if (database.AaPanel == null)
        {
            database = DatabaseRepository
                .Get()
                .Include(x => x.AaPanel)
                .First(x => x.Id == database.Id);
        }
        
        return new aaPanel(database.AaPanel.Url, database.AaPanel.Key);
    }
}