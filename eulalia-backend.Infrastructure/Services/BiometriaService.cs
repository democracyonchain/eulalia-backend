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
            var keyFromConfig =
                Environment.GetEnvironmentVariable("BIOMETRIA_ENCRYPTION_KEY") ??
                config["Biometria:EncryptionKey"];
            if (string.IsNullOrWhiteSpace(keyFromConfig))
                throw new InvalidOperationException("Biometria:EncryptionKey no está configurada.");

            _encryptionKey = Encoding.UTF8.GetBytes(keyFromConfig);
            if (_encryptionKey.Length != 16 && _encryptionKey.Length != 24 && _encryptionKey.Length != 32)
                throw new InvalidOperationException("Biometria:EncryptionKey debe tener 16, 24 o 32 bytes.");
        }

        public async Task RegistrarBiometriaAsync(string cedula, byte[] template, string estadoInicial = "pendiente")
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
                Estadoverificacion = estadoInicial,
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
            // AES-GCM envelope: [12-byte nonce][16-byte tag][ciphertext]
            var nonce = RandomNumberGenerator.GetBytes(12);
            var tag = new byte[16];
            var ciphertext = new byte[data.Length];

            using var aesGcm = new AesGcm(_encryptionKey, 16);
            aesGcm.Encrypt(nonce, data, ciphertext, tag);

            var resultado = new byte[nonce.Length + tag.Length + ciphertext.Length];
            Buffer.BlockCopy(nonce, 0, resultado, 0, nonce.Length);
            Buffer.BlockCopy(tag, 0, resultado, nonce.Length, tag.Length);
            Buffer.BlockCopy(ciphertext, 0, resultado, nonce.Length + tag.Length, ciphertext.Length);

            return resultado;
        }
    }
}
