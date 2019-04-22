@echo off
cl test.c %Platform%\liblua53.a /I include /link /OUT:%Platform%\test.exe