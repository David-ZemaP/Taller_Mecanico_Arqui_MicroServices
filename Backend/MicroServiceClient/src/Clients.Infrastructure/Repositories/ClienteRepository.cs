using Google.Cloud.Firestore;
using Taller_Mecanico_Clientes.Domain.Common;
using Taller_Mecanico_Clientes.Domain.Entities;
using Taller_Mecanico_Clientes.Domain.Interfaces;
using Taller_Mecanico_Clientes.Infrastructure.Persistence;

namespace Taller_Mecanico_Clientes.Infrastructure.Repositories
{
    public class ClienteRepository : IClienteRepository
    {
        private readonly FirestoreDb _firestoreDb;
        private const string CollectionName = "clientes";

        public ClienteRepository(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<Result<List<Cliente>>> GetAllAsync()
        {
            try
            {
                var collection = _firestoreDb.Collection(CollectionName);
                var query = collection.WhereEqualTo("isDeleted", false);
                var snapshot = await query.GetSnapshotAsync();
                
                var clientes = snapshot.Documents.Select(doc =>
                {
                    var docModel = doc.ConvertTo<ClienteDocument>();
                    return docModel.ToEntity();
                }).ToList();

                return Result<List<Cliente>>.Success(clientes);
            }
            catch (Exception ex)
            {
                return Result<List<Cliente>>.Failure(ErrorCodes.DbError, $"Error al obtener clientes: {ex.Message}");
            }
        }

        public async Task<Result<Cliente?>> GetByIdAsync(string id)
        {
            try
            {
                var doc = await _firestoreDb.Collection(CollectionName).Document(id).GetSnapshotAsync();
                if (!doc.Exists)
                {
                    return Result<Cliente?>.Success(null);
                }

                var docModel = doc.ConvertTo<ClienteDocument>();
                if (docModel.IsDeleted)
                {
                    return Result<Cliente?>.Success(null);
                }

                return Result<Cliente?>.Success(docModel.ToEntity());
            }
            catch (Exception ex)
            {
                return Result<Cliente?>.Failure(ErrorCodes.DbError, $"Error al obtener cliente por ID: {ex.Message}");
            }
        }

        public async Task<Result<Cliente>> CreateAsync(Cliente cliente)
        {
            try
            {
                var docModel = ClienteDocument.FromEntity(cliente);
                var docRef = _firestoreDb.Collection(CollectionName).Document();
                docModel.Id = docRef.Id;

                await docRef.SetAsync(docModel);
                
                cliente.Id = docModel.Id;
                return Result<Cliente>.Success(cliente);
            }
            catch (Exception ex)
            {
                return Result<Cliente>.Failure(ErrorCodes.DbError, $"Error al crear cliente: {ex.Message}");
            }
        }

        public async Task<Result<Cliente>> UpdateAsync(string id, Cliente cliente)
        {
            try
            {
                var docRef = _firestoreDb.Collection(CollectionName).Document(id);
                var snapshot = await docRef.GetSnapshotAsync();
                if (!snapshot.Exists)
                {
                    return Result<Cliente>.Failure(ErrorCodes.ClienteNotFound, "El cliente a actualizar no existe.");
                }

                var docModelExisting = snapshot.ConvertTo<ClienteDocument>();
                if (docModelExisting.IsDeleted)
                {
                    return Result<Cliente>.Failure(ErrorCodes.ClienteNotFound, "El cliente a actualizar ha sido eliminado.");
                }

                cliente.Id = id;
                cliente.FechaRegistro = docModelExisting.FechaRegistro; // Preserve original creation date
                cliente.FechaActualizacion = DateTime.UtcNow;

                var docModel = ClienteDocument.FromEntity(cliente);
                await docRef.SetAsync(docModel, SetOptions.Overwrite);

                return Result<Cliente>.Success(cliente);
            }
            catch (Exception ex)
            {
                return Result<Cliente>.Failure(ErrorCodes.DbError, $"Error al actualizar cliente: {ex.Message}");
            }
        }

        public async Task<Result> DeleteAsync(string id)
        {
            try
            {
                var docRef = _firestoreDb.Collection(CollectionName).Document(id);
                var snapshot = await docRef.GetSnapshotAsync();
                if (!snapshot.Exists)
                {
                    return Result.Failure(ErrorCodes.ClienteNotFound, "El cliente a eliminar no existe.");
                }

                var docModel = snapshot.ConvertTo<ClienteDocument>();
                if (docModel.IsDeleted)
                {
                    return Result.Failure(ErrorCodes.ClienteNotFound, "El cliente ya se encuentra eliminado.");
                }

                // Perform soft delete
                var updates = new Dictionary<string, object>
                {
                    { "isDeleted", true },
                    { "fechaActualizacion", DateTime.UtcNow }
                };
                await docRef.UpdateAsync(updates);
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(ErrorCodes.DbError, $"Error al eliminar cliente (soft delete): {ex.Message}");
            }
        }
    }
}
