using Solution2.Model;
using System.Text.Json;
using System.Text.Json.Serialization;
using Group = Solution2.Model.Group;

namespace Solution2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Activate Endpoints for OpenAPI
            builder.Services.AddEndpointsApiExplorer();
            // Add OpenAPI Services
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                // Add Swagger and SwaggerUI
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            List<Group> groups = new List<Group>();
            var jsonOptions = new JsonSerializerOptions()
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };


            app.MapPost("/group", (Group group) =>
            {
                var name = (group.Name.Length == 0) ? null : group.Name;
                if (name == null) return Results.BadRequest();

                groups.Add(group);
                return Results.Ok(group.Id);
            })
                .Produces<int>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .WithName("CreateGroup")
                .WithTags("Post");

            app.MapGet("/groups", () =>
            {
                var shortGroup = (
                from item in groups
                select new Group()
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    Participants = null,
                }).ToList();

                return Results.Json(shortGroup, jsonOptions, "application/json", StatusCodes.Status200OK);
            })
                .Produces<List<Group>>(StatusCodes.Status200OK)
                .WithName("GetAllGroups")
                .WithTags("Get");

            app.MapGet("/group/{id}", (int id) =>
            {
                var shortGroupById = (
                from item in groups
                select new Group()
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    Participants = 
                    (from item2 in item.Participants
                                    select new Participant()
                                    {
                                        Id = item2.Id,
                                        Name = item2.Name,
                                        Wish = item2.Wish,
                                        Recipient = (item2.Recipient != null)
                                        ? new Participant
                                        {
                                            Id = item2.Recipient.Id,
                                            Name = item2.Recipient.Name,
                                            Wish = item2.Recipient.Wish,
                                            Recipient = null
                                        }
                                        : null ,
                                    }).ToList()
                    
                }).ToList();

                return Results.Json(shortGroupById, jsonOptions, "application/json", StatusCodes.Status200OK);
            })
                .Produces<Group>(StatusCodes.Status200OK)
                .WithName("GetGroupById")
                .WithTags("Get");

            app.MapPut("/group/{id}", (int id, Group group) =>
            {
                var oldGroup = groups.FirstOrDefault(g => g.Id == id);
                if (oldGroup == null) return Results.NotFound();
                var name = (group.Name.Length == 0) ? oldGroup.Name : group.Name;
                var descr = group.Description;
                oldGroup.Name = name;
                oldGroup.Description = descr;
                return Results.NoContent();
            })
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound)
                .WithName("UpdateGroupById")
                .WithTags("Put");

            app.MapDelete("/group/{id}", (int id) =>
            {
                Group grp = groups.FirstOrDefault(g => g.Id == id);
                if (grp == null) return Results.NotFound();
                groups.Remove(grp);
                return Results.NoContent();
            })
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound)
                .WithName("DeleteGroupById")
                .WithTags("Delete");

            app.MapPost("/group/{id}/participant", (int groupId, Participant participant) =>
            {
                var grp = groups.FirstOrDefault(g => g.Id == groupId);
                if (grp == null) return Results.NotFound();
                var name = (participant.Name.Length == 0) ? null : participant.Name;
                if (name == null) return Results.NotFound();
                var wish = participant.Wish; //todo delete??
                grp.Participants.Add(participant);
                return Results.Ok(participant.Id);
            })
                .Produces<int>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .WithName("CreateParticipantByGroupId")
                .WithTags("Post");

            app.MapDelete("/group/{groupId}/participant/{participantId}",
                (int groupId, int participantId) =>
            {
                var grp = groups.FirstOrDefault(g => g.Id == groupId);
                if (grp == null) return Results.NotFound();
                var prtc = grp.Participants.FirstOrDefault(p => p.Id == participantId);
                if (prtc == null) return Results.NotFound();

                grp.Participants.Remove(prtc);
                return Results.NoContent();
            })
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound)
                .WithName("DeleteParticipantById")
                .WithTags("Delete");

            app.MapPost("/group/{id}/toss", (int id) =>
            {
                var grp = groups.FirstOrDefault(g => g.Id == id);

                if (grp == null) return Results.NotFound();

                var prtc = grp.Participants;

                if (grp.Participants.Count >= 3)
                {
                    Toss(ref grp);
                    var resultList = (
                from item in prtc
                select new Participant()
                {
                    Id = item.Id,
                    Name = item.Name,
                    Wish = item.Wish,
                    Recipient = new Participant
                    {
                        Id = item.Recipient.Id,
                        Name = item.Recipient.Name,
                        Wish = item.Recipient.Wish,
                        Recipient = null
                    },
                }).ToList();

                    return Results.Json(resultList, jsonOptions, "application/json", StatusCodes.Status200OK);
                }
                return Results.Conflict();
            })
                .Produces<List<Participant>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status409Conflict)
                .Produces(StatusCodes.Status404NotFound)
                .WithName("TossByGroupId")
                .WithTags("Post");


            app.MapGet("/group/{groupId}/participant/{participantId}/recipient",
                (int groupId, int participantId) =>
            {
                var grp = groups.FirstOrDefault(g => g.Id == groupId);
                if (grp == null) return Results.NotFound();
                var prtc = grp.Participants.FirstOrDefault(p => p.Id == participantId);
                if (prtc == null) return Results.NotFound();
                var recip = new Participant
                {
                    Id = prtc.Recipient.Id,
                    Name = prtc.Recipient.Name,
                    Wish = prtc.Recipient.Wish,
                    Recipient = null
                };
                return Results.Json(recip, jsonOptions, "application/json", StatusCodes.Status200OK);
            })
                .Produces<Participant>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .WithName("GetRecipientByParticipantId")
                .WithTags("Get"); ;

            app.UseHttpsRedirection();

            app.Run();
        }

        public static void Toss(ref Group grp)
        {
            for (int i = 0; i < grp.Participants.Count; i++)
            {
                if (i == grp.Participants.Count - 1)
                {
                    grp.Participants[i].Recipient = grp.Participants[0];
                    break;
                }
                Participant currentParticipant = grp.Participants[i];
                currentParticipant.Recipient = grp.Participants[i + 1];
            }
        }
    }
}