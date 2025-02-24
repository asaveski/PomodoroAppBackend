using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PomodoroAppBackend.Migrations
{
    /// <inheritdoc />
    public partial class OwnedEntitiesTry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cues_Notes_NoteId",
                table: "Cues");

            migrationBuilder.DropForeignKey(
                name: "FK_Notes_Subjects_SubjectId",
                table: "Notes");

            migrationBuilder.DropForeignKey(
                name: "FK_SuccinctNotes_Notes_NoteId",
                table: "SuccinctNotes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SuccinctNotes",
                table: "SuccinctNotes");

            migrationBuilder.DropIndex(
                name: "IX_SuccinctNotes_NoteId",
                table: "SuccinctNotes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Cues",
                table: "Cues");

            migrationBuilder.DropIndex(
                name: "IX_Cues_NoteId",
                table: "Cues");

            migrationBuilder.RenameTable(
                name: "SuccinctNotes",
                newName: "SuccinctNote");

            migrationBuilder.RenameTable(
                name: "Cues",
                newName: "Cue");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SuccinctNote",
                table: "SuccinctNote",
                columns: new[] { "NoteId", "SuccinctNoteId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Cue",
                table: "Cue",
                columns: new[] { "NoteId", "CueId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Cue_Notes_NoteId",
                table: "Cue",
                column: "NoteId",
                principalTable: "Notes",
                principalColumn: "NoteId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_Subjects_SubjectId",
                table: "Notes",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "SubjectId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SuccinctNote_Notes_NoteId",
                table: "SuccinctNote",
                column: "NoteId",
                principalTable: "Notes",
                principalColumn: "NoteId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cue_Notes_NoteId",
                table: "Cue");

            migrationBuilder.DropForeignKey(
                name: "FK_Notes_Subjects_SubjectId",
                table: "Notes");

            migrationBuilder.DropForeignKey(
                name: "FK_SuccinctNote_Notes_NoteId",
                table: "SuccinctNote");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SuccinctNote",
                table: "SuccinctNote");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Cue",
                table: "Cue");

            migrationBuilder.RenameTable(
                name: "SuccinctNote",
                newName: "SuccinctNotes");

            migrationBuilder.RenameTable(
                name: "Cue",
                newName: "Cues");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SuccinctNotes",
                table: "SuccinctNotes",
                column: "SuccinctNoteId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Cues",
                table: "Cues",
                column: "CueId");

            migrationBuilder.CreateIndex(
                name: "IX_SuccinctNotes_NoteId",
                table: "SuccinctNotes",
                column: "NoteId");

            migrationBuilder.CreateIndex(
                name: "IX_Cues_NoteId",
                table: "Cues",
                column: "NoteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cues_Notes_NoteId",
                table: "Cues",
                column: "NoteId",
                principalTable: "Notes",
                principalColumn: "NoteId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_Subjects_SubjectId",
                table: "Notes",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "SubjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_SuccinctNotes_Notes_NoteId",
                table: "SuccinctNotes",
                column: "NoteId",
                principalTable: "Notes",
                principalColumn: "NoteId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
