using FirebaseAdmin.Messaging;
using FirebaseAdmin;
using Google.Apis.Firestore.v1;
using Google.Apis.Services;
using Google.Cloud.Firestore;
using MicroServiceClient.Models;

public interface IFirebaseService
{
    Task<List<Cliente>> GetAllClientesAsync();
    Task<Cliente?> GetClienteByIdAsync(string id);
    Task<Cliente> CreateClienteAsync(Cliente cliente);
    Task<Cliente> UpdateClienteAsync(string id, Cliente cliente);
    Task DeleteClienteAsync(string id);
}

public class FirebaseService : IFirebaseService
{
    private readonly FirestoreDb _firestore;

    public FirebaseService()
    {
        _firestore = FirestoreDb.Create(FirebaseApp.DefaultInstance!.Options.ProjectId);
    }

    public async Task<List<Cliente>> GetAllClientesAsync()
    {
        var collection = _firestore.Collection("clientes");
        var snapshot = await collection.GetSnapshotAsync();
        
        return snapshot.Documents.Select(doc =>
        {
            var cliente = doc.ConvertTo<Cliente>();
            cliente.Id = doc.Id;
            return cliente;
        }).ToList();
    }

    public async Task<Cliente?> GetClienteByIdAsync(string id)
    {
        var doc = await _firestore.Collection("clientes").Document(id).GetSnapshotAsync();
        
        if (!doc.Exists)
            return null;
            
        var cliente = doc.ConvertTo<Cliente>();
        cliente.Id = doc.Id;
        return cliente;
    }

    public async Task<Cliente> CreateClienteAsync(Cliente cliente)
    {
        cliente.FechaCreacion = DateTime.UtcNow;
        
        var docRef = await _firestore.Collection("clientes").AddAsync(cliente);
        cliente.Id = docRef.Id;
        return cliente;
    }

    public async Task<Cliente> UpdateClienteAsync(string id, Cliente cliente)
    {
        var docRef = _firestore.Collection("clientes").Document(id);
        await docRef.SetAsync(cliente, SetOptions.Overwrite());
        cliente.Id = id;
        return cliente;
    }

    public async Task DeleteClienteAsync(string id)
    {
        await _firestore.Collection("clientes").Document(id).DeleteAsync();
    }
}