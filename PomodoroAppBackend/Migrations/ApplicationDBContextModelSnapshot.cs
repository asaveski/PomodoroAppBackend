﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PomodoroAppBackend.Context;

#nullable disable

namespace PomodoroAppBackend.Migrations
{
    [DbContext(typeof(ApplicationDBContext))]
    partial class ApplicationDBContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("PomodoroAppBackend.Models.Note", b =>
                {
                    b.Property<int>("NoteId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("NoteId"));

                    b.Property<int?>("SubjectId")
                        .HasColumnType("int");

                    b.Property<string>("Summary")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Topic")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("NoteId");

                    b.HasIndex("SubjectId");

                    b.ToTable("Notes");
                });

            modelBuilder.Entity("PomodoroAppBackend.Models.Subject", b =>
                {
                    b.Property<int>("SubjectId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("SubjectId"));

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("SubjectId");

                    b.ToTable("Subjects");
                });

            modelBuilder.Entity("PomodoroAppBackend.Models.Note", b =>
                {
                    b.HasOne("PomodoroAppBackend.Models.Subject", "Subject")
                        .WithMany()
                        .HasForeignKey("SubjectId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.OwnsMany("PomodoroAppBackend.Models.Cue", "Cues", b1 =>
                        {
                            b1.Property<int>("NoteId")
                                .HasColumnType("int");

                            b1.Property<int>("CueId")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("int");

                            SqlServerPropertyBuilderExtensions.UseIdentityColumn(b1.Property<int>("CueId"));

                            b1.Property<string>("Text")
                                .HasColumnType("nvarchar(max)");

                            b1.HasKey("NoteId", "CueId");

                            b1.ToTable("Cue");

                            b1.WithOwner()
                                .HasForeignKey("NoteId");
                        });

                    b.OwnsMany("PomodoroAppBackend.Models.Quiz", "Quizzes", b1 =>
                        {
                            b1.Property<int>("QuizId")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("int");

                            SqlServerPropertyBuilderExtensions.UseIdentityColumn(b1.Property<int>("QuizId"));

                            b1.Property<int>("NoteId")
                                .HasColumnType("int");

                            b1.HasKey("QuizId");

                            b1.HasIndex("NoteId");

                            b1.ToTable("Quiz");

                            b1.WithOwner()
                                .HasForeignKey("NoteId");

                            b1.OwnsMany("PomodoroAppBackend.Models.Question", "Questions", b2 =>
                                {
                                    b2.Property<int>("QuestionId")
                                        .ValueGeneratedOnAdd()
                                        .HasColumnType("int");

                                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b2.Property<int>("QuestionId"));

                                    b2.Property<string>("CorrectAnswer")
                                        .HasColumnType("nvarchar(max)");

                                    b2.PrimitiveCollection<string>("Options")
                                        .HasColumnType("nvarchar(max)");

                                    b2.Property<string>("QuestionText")
                                        .HasColumnType("nvarchar(max)");

                                    b2.Property<int>("QuizId")
                                        .HasColumnType("int");

                                    b2.HasKey("QuestionId");

                                    b2.HasIndex("QuizId");

                                    b2.ToTable("Questions");

                                    b2.WithOwner()
                                        .HasForeignKey("QuizId");
                                });

                            b1.Navigation("Questions");
                        });

                    b.OwnsMany("PomodoroAppBackend.Models.SuccinctNote", "SuccinctNotes", b1 =>
                        {
                            b1.Property<int>("NoteId")
                                .HasColumnType("int");

                            b1.Property<int>("SuccinctNoteId")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("int");

                            SqlServerPropertyBuilderExtensions.UseIdentityColumn(b1.Property<int>("SuccinctNoteId"));

                            b1.Property<string>("Summary")
                                .HasColumnType("nvarchar(max)");

                            b1.HasKey("NoteId", "SuccinctNoteId");

                            b1.ToTable("SuccinctNote");

                            b1.WithOwner()
                                .HasForeignKey("NoteId");
                        });

                    b.Navigation("Cues");

                    b.Navigation("Quizzes");

                    b.Navigation("Subject");

                    b.Navigation("SuccinctNotes");
                });
#pragma warning restore 612, 618
        }
    }
}
