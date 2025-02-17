using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PomodoroAppBackend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNoteCueSubjectSuccinctNote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cue",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Notes");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Notes",
                newName: "NoteId");

            migrationBuilder.AlterColumn<string>(
                name: "Topic",
                table: "Notes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Summary",
                table: "Notes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "SubjectId",
                table: "Notes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Cues",
                columns: table => new
                {
                    CueId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoteId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cues", x => x.CueId);
                    table.ForeignKey(
                        name: "FK_Cues_Notes_NoteId",
                        column: x => x.NoteId,
                        principalTable: "Notes",
                        principalColumn: "NoteId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subjects",
                columns: table => new
                {
                    SubjectId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subjects", x => x.SubjectId);
                });

            migrationBuilder.CreateTable(
                name: "SuccinctNotes",
                columns: table => new
                {
                    SuccinctNoteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoteId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuccinctNotes", x => x.SuccinctNoteId);
                    table.ForeignKey(
                        name: "FK_SuccinctNotes_Notes_NoteId",
                        column: x => x.NoteId,
                        principalTable: "Notes",
                        principalColumn: "NoteId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notes_SubjectId",
                table: "Notes",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Cues_NoteId",
                table: "Cues",
                column: "NoteId");

            migrationBuilder.CreateIndex(
                name: "IX_SuccinctNotes_NoteId",
                table: "SuccinctNotes",
                column: "NoteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_Subjects_SubjectId",
                table: "Notes",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "SubjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notes_Subjects_SubjectId",
                table: "Notes");

            migrationBuilder.DropTable(
                name: "Cues");

            migrationBuilder.DropTable(
                name: "Subjects");

            migrationBuilder.DropTable(
                name: "SuccinctNotes");

            migrationBuilder.DropIndex(
                name: "IX_Notes_SubjectId",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "SubjectId",
                table: "Notes");

            migrationBuilder.RenameColumn(
                name: "NoteId",
                table: "Notes",
                newName: "Id");

            migrationBuilder.AlterColumn<string>(
                name: "Topic",
                table: "Notes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Summary",
                table: "Notes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Cue",
                table: "Notes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Notes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
