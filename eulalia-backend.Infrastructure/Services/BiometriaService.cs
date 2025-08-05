using eulalia_backend.Domain.Entities;
using eulalia_backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace eulalia_backend.Infrastructure.Services
{
    public class BiometriaService
    {
        private readonly EulaliaContext _context;
        private readonly byte[] _encryptionKey;

        public BiometriaService(EulaliaContext context, IConfiguration config)
        {
            _context = context;
            _encryptionKey = Encoding.UTF8.GetBytes(config["Biometria:EncryptionKey"]);
        }

        public async Task RegistrarBiometriaAsync(string cedula, byte[] template)
        {
            if (await _context.BiometriasCiudadano.AnyAsync(b => b.Cedula == cedula))
                throw new InvalidOperationException("Ya existe un registro biométrico para esta cédula.");

            var hash = GenerarHash(template);
            var cifrado = CifrarAES(template);

            var biometria = new BiometriaCiudadano
            {
                Cedula = cedula,
                Templatecifrado = cifrado,
                Hashtemplate = hash,
                Estadoverificacion = "pendiente",
                Fecharegistro = DateTime.UtcNow
            };

            _context.BiometriasCiudadano.Add(biometria);
            await _context.SaveChangesAsync();
        }

        public async Task<BiometriaCiudadano?> ObtenerPorCedulaAsync(string cedula)
        {
            return await _context.BiometriasCiudadano
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Cedula == cedula);
        }

        public async Task ActualizarEstadoAsync(string cedula, string nuevoEstado)
        {
            var biometria = await _context.BiometriasCiudadano.FirstOrDefaultAsync(b => b.Cedula == cedula);
            if (biometria == null) throw new KeyNotFoundException("Registro no encontrado.");

            biometria.Estadoverificacion = nuevoEstado;
            await _context.SaveChangesAsync();
        }

        private string GenerarHash(byte[] data)
        {
            using var sha256 = SHA256.Create();
            return Convert.ToBase64String(sha256.ComputeHash(data));
        }

        private byte[] CifrarAES(byte[] data)
        {
            using var aes = Aes.Create();
            aes.Key = _encryptionKey;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            var cifrado = encryptor.TransformFinalBlock(data, 0, data.Length);

            var resultado = new byte[aes.IV.Length + cifrado.Length];
            Buffer.BlockCopy(aes.IV, 0, resultado, 0, aes.IV.Length);
            Buffer.BlockCopy(cifrado, 0, resultado, aes.IV.Length, cifrado.Length);

            return resultado;
        }
    }
}
