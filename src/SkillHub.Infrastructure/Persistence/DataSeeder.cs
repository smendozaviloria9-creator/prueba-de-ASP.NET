using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SkillHub.Application.Interfaces;
using SkillHub.Domain.Constants;
using SkillHub.Domain.Entities;

namespace SkillHub.Infrastructure.Persistence;

public class DataSeeder : IDataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public DataSeeder(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        // 1. Seed Roles
        string[] roles = [UserRoles.Employee, UserRoles.Participant];
        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // 2. Seed Instructors (Employees)
        var instructorsData = new List<(string Name, string Email, string Password, string Specialty)>
        {
            ("Andrés Felipe Mejía Arango", "andres.mejia@correo.com", "Instructor2026*", "Fotografía"),
            ("Rosa Elena Barrios Polo", "rosa.barrios@correo.com", "Instructor2026*", "Cocina"),
            ("Mauricio Jiménez Cárdenas", "mauricio.jimenez@correo.com", "Instructor2026*", "Oratoria"),
            ("Natalia Herrera Castaño", "natalia.herrera@correo.com", "Instructor2026*", "Diseño gráfico"),
            ("Hernán Darío Quintero Gil", "hernan.quintero@correo.com", "Instructor2026*", "Música"),
            ("Paola Andrea Vargas Mora", "paola.vargas@correo.com", "Instructor2026*", "Marketing digital")
        };

        foreach (var inst in instructorsData)
        {
            var user = await _userManager.FindByEmailAsync(inst.Email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = inst.Email,
                    Email = inst.Email,
                    FullName = inst.Name,
                    Specialty = inst.Specialty,
                    Role = UserRoles.Employee,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var createRes = await _userManager.CreateAsync(user, inst.Password);
                if (createRes.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, UserRoles.Employee);
                }
            }
        }

        // 3. Seed Participants
        var participantsData = new List<(string Name, string Email, string Password)>
        {
            ("Jorge Andrés Pérez Rojas", "jorge.perez@correo.com", "Skillhub2026*"),
            ("Luz Marina Ospina Ríos", "luz.ospina@correo.com", "Skillhub2026*"),
            ("Carlos Eduardo Rincón Díaz", "carlos.rincon@correo.com", "Skillhub2026*"),
            ("Valentina Gómez Ruiz", "valentina.gomez@correo.com", "Skillhub2026*"),
            ("Juan Camilo Restrepo Vélez", "juan.restrepo@correo.com", "Skillhub2026*"),
            ("María Fernanda Torres León", "maria.torres@correo.com", "Skillhub2026*"),
            ("Santiago Ramírez Henao", "santiago.ramirez@correo.com", "Skillhub2026*"),
            ("Daniela Morales Pineda", "daniela.morales@correo.com", "Skillhub2026*"),
            ("Sebastián Castro Duque", "sebastian.castro@correo.com", "Skillhub2026*"),
            ("Laura Sofía Martínez Suárez", "laura.martinez@correo.com", "Skillhub2026*"),
            ("Andrés Mauricio Salazar Ortiz", "andres.salazar@correo.com", "Skillhub2026*"),
            ("Camila Andrea Cárdenas Ruiz", "camila.cardenas@correo.com", "Skillhub2026*"),
            ("Diego Alejandro Muñoz Peña", "diego.munoz@correo.com", "Skillhub2026*"),
            ("Juliana Ortega Sánchez", "juliana.ortega@correo.com", "Skillhub2026*"),
            ("Felipe Agudelo Marín", "felipe.agudelo@correo.com", "Skillhub2026*"),
            ("Natalia Montoya Correa", "natalia.montoya@correo.com", "Skillhub2026*"),
            ("Óscar Iván Guerrero Lozano", "oscar.guerrero@correo.com", "Skillhub2026*"),
            ("Paula Andrea Zapata Ríos", "paula.zapata@correo.com", "Skillhub2026*"),
            ("Cristian David Becerra Mejía", "cristian.becerra@correo.com", "Skillhub2026*"),
            ("Mariana Londoño Álvarez", "mariana.londono@correo.com", "Skillhub2026*"),
            ("Javier Alonso Cuéllar Pardo", "javier.cuellar@correo.com", "Skillhub2026*"),
            ("Isabella Navarro Fuentes", "isabella.navarro@correo.com", "Skillhub2026*"),
            ("Kevin Stiven Mosquera Palacios", "kevin.mosquera@correo.com", "Skillhub2026*"),
            ("Ana María Jaramillo Gaviria", "ana.jaramillo@correo.com", "Skillhub2026*"),
            ("Luis Fernando Arias Cano", "luis.arias@correo.com", "Skillhub2026*"),
            ("Sara Valencia Bermúdez", "sara.valencia@correo.com", "Skillhub2026*"),
            ("Esteban Rodríguez Molina", "esteban.rodriguez@correo.com", "Skillhub2026*"),
            ("Manuela Escobar Toro", "manuela.escobar@correo.com", "Skillhub2026*"),
            ("Wilmer Alfonso Benítez Cruz", "wilmer.benitez@correo.com", "Skillhub2026*"),
            ("Yuliana Patiño Osorio", "yuliana.patino@correo.com", "Skillhub2026*")
        };

        foreach (var part in participantsData)
        {
            var user = await _userManager.FindByEmailAsync(part.Email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = part.Email,
                    Email = part.Email,
                    FullName = part.Name,
                    Role = UserRoles.Participant,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var createRes = await _userManager.CreateAsync(user, part.Password);
                if (createRes.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, UserRoles.Participant);
                }
            }
        }

        // 4. Seed Workshops
        var workshopsData = new List<(string Code, string Title, string Description, string InstructorEmail, DateTime Start, DateTime End, int Capacity)>
        {
            ("FOT-001", "Fotografía con celular", "Composición, luz y edición básica desde el celular", "andres.mejia@correo.com", DateTime.Parse("2026-12-05 08:00:00"), DateTime.Parse("2026-12-05 12:00:00"), 15),
            ("FOT-002", "Retrato en exteriores", "Manejo de luz natural para retratos al aire libre", "andres.mejia@correo.com", DateTime.Parse("2027-01-16 08:00:00"), DateTime.Parse("2027-01-16 12:00:00"), 10),
            ("FOT-003", "Edición en Lightroom", "Flujo de revelado y ajustes de color", "andres.mejia@correo.com", DateTime.Parse("2027-02-06 14:00:00"), DateTime.Parse("2027-02-06 18:00:00"), 12),
            ("COC-001", "Cocina costeña", "Arroz con coco, patacones y mojarra frita", "rosa.barrios@correo.com", DateTime.Parse("2026-12-05 14:00:00"), DateTime.Parse("2026-12-05 18:00:00"), 12),
            ("COC-002", "Repostería básica", "Tortas, galletas y postres de nevera", "rosa.barrios@correo.com", DateTime.Parse("2027-01-09 09:00:00"), DateTime.Parse("2027-01-09 13:00:00"), 10),
            ("COC-003", "Cocina saludable", "Platos balanceados para la semana", "rosa.barrios@correo.com", DateTime.Parse("2027-02-13 09:00:00"), DateTime.Parse("2027-02-13 12:00:00"), 14),
            ("ORA-001", "Oratoria para principiantes", "Técnicas para hablar en público con seguridad", "mauricio.jimenez@correo.com", DateTime.Parse("2026-12-12 09:00:00"), DateTime.Parse("2026-12-12 12:00:00"), 20),
            ("ORA-002", "Presentaciones efectivas", "Cómo estructurar y presentar una idea en 10 minutos", "mauricio.jimenez@correo.com", DateTime.Parse("2027-01-23 09:00:00"), DateTime.Parse("2027-01-23 12:00:00"), 18),
            ("DIS-001", "Diseño en Canva", "Piezas gráficas para redes sociales", "natalia.herrera@correo.com", DateTime.Parse("2026-12-19 14:00:00"), DateTime.Parse("2026-12-19 17:00:00"), 18),
            ("DIS-002", "Fundamentos de color y tipografía", "Principios visuales para piezas profesionales", "natalia.herrera@correo.com", DateTime.Parse("2027-01-30 14:00:00"), DateTime.Parse("2027-01-30 17:00:00"), 15),
            ("MUS-001", "Guitarra desde cero", "Acordes básicos y primeras canciones", "hernan.quintero@correo.com", DateTime.Parse("2026-12-12 14:00:00"), DateTime.Parse("2026-12-12 17:00:00"), 10),
            ("MUS-002", "Producción musical en casa", "Grabación y mezcla con herramientas gratuitas", "hernan.quintero@correo.com", DateTime.Parse("2027-02-20 14:00:00"), DateTime.Parse("2027-02-20 18:00:00"), 12),
            ("MKT-001", "Marketing en redes sociales", "Estrategia de contenido para pequeños negocios", "paola.vargas@correo.com", DateTime.Parse("2026-12-19 08:00:00"), DateTime.Parse("2026-12-19 12:00:00"), 20),
            ("MKT-002", "Publicidad en Meta e Instagram", "Configuración y medición de campañas", "paola.vargas@correo.com", DateTime.Parse("2027-01-16 14:00:00"), DateTime.Parse("2027-01-16 18:00:00"), 16),
            ("MKT-003", "Emprendimiento digital", "Validar y lanzar una idea de negocio en línea", "paola.vargas@correo.com", DateTime.Parse("2027-02-27 09:00:00"), DateTime.Parse("2027-02-27 12:00:00"), 20)
        };

        foreach (var ws in workshopsData)
        {
            var exists = await _context.Workshops.AnyAsync(w => w.Code == ws.Code, cancellationToken);
            if (!exists)
            {
                var instructor = await _userManager.FindByEmailAsync(ws.InstructorEmail);
                if (instructor != null)
                {
                    var workshop = new Workshop
                    {
                        Code = ws.Code,
                        Title = ws.Title,
                        Description = ws.Description,
                        InstructorId = instructor.Id,
                        StartDateTime = DateTime.SpecifyKind(ws.Start, DateTimeKind.Utc),
                        EndDateTime = DateTime.SpecifyKind(ws.End, DateTimeKind.Utc),
                        Capacity = ws.Capacity,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _context.Workshops.AddAsync(workshop, cancellationToken);
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
