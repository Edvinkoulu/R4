using System.Data;
using System.Diagnostics;
using MySqlConnector;
using Village_Newbies.Models;
using Village_Newbies.Interfacet;
using Village_Newbies.Services;
using DatabaseConnection;


namespace Village_Newbies.Services
{
    public class VarausDatabaseService : DatabaseService
    {
        public VarausDatabaseService() { }

        public VarausDatabaseService(DatabaseConnector connector) : base(connector) { }

        public override async Task<DataTable> HaeData(string sql, params (string, object)[] parameters)
        {
            using var conn = HaeYhteysTietokantaan();
            using var cmd = new MySqlCommand(sql, conn);
            foreach (var (nimi, arvo) in parameters) cmd.Parameters.AddWithValue(nimi, arvo);

            var table = new DataTable();
            using var adapter = new MySqlDataAdapter(cmd);
            await Task.Run(() => adapter.Fill(table));
            return table;
        }

        public override async Task<int> SuoritaKomento(string sql, params (string, object)[] parameters)
        {
            using var conn = HaeYhteysTietokantaan();
            using var cmd = new MySqlCommand(sql, conn);
            foreach (var (nimi, arvo) in parameters) cmd.Parameters.AddWithValue(nimi, arvo);

            return await cmd.ExecuteNonQueryAsync();
        }

        // ==================== CRUD =======================

        public async Task<List<Varaus>> HaeKaikki()
        {
            var data = await HaeData("SELECT * FROM varaus");
            return LuoLista(data);
        }

        public async Task<Varaus?> Hae(int id)
        {
            var data = await HaeData("SELECT * FROM varaus WHERE varaus_id = @id", ("@id", id));
            return data.Rows.Count > 0 ? LuoOlio(data.Rows[0]) : null;
        }

        public async Task Lisaa(Varaus varaus)
        {
            var sql = @"INSERT INTO varaus 
                        (asiakas_id, mokki_id, varattu_pvm, vahvistus_pvm, varattu_alkupvm, varattu_loppupvm)
                        VALUES 
                        (@asiakas_id, @mokki_id, @varattu_pvm, @vahvistus_pvm, @varattu_alkupvm, @varattu_loppupvm)";
            await SuoritaKomento(sql,
                ("@asiakas_id", varaus.asiakas_id),
                ("@mokki_id", varaus.mokki_id),
                ("@varattu_pvm", varaus.varattu_pvm),
                ("@vahvistus_pvm", varaus.vahvistus_pvm ?? (object)DBNull.Value),
                ("@varattu_alkupvm", varaus.varattu_alkupvm),
                ("@varattu_loppupvm", varaus.varattu_loppupvm));
        }

        public async Task Poista(int id)
        {
            await SuoritaKomento("DELETE FROM varaus WHERE varaus_id = @id", ("@id", id));
        }

        // ==================== APUMETODIT =======================

        private Varaus LuoOlio(DataRow r)
        {
            return new Varaus
            {
                varaus_id = Convert.ToUInt32(r["varaus_id"]),
                asiakas_id = Convert.ToUInt32(r["asiakas_id"]),
                mokki_id = Convert.ToUInt32(r["mokki_id"]),
                varattu_pvm = Convert.ToDateTime(r["varattu_pvm"]),
                vahvistus_pvm = r["vahvistus_pvm"] is DBNull ? null : Convert.ToDateTime(r["vahvistus_pvm"]),
                varattu_alkupvm = Convert.ToDateTime(r["varattu_alkupvm"]),
                varattu_loppupvm = Convert.ToDateTime(r["varattu_loppupvm"])
            };
        }

        private List<Varaus> LuoLista(DataTable taulu)
            => taulu.AsEnumerable().Select(r => LuoOlio(r)).ToList();
    }
}