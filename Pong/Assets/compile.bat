@echo off
cd images
set atlas=atlas
for %%n in (%atlas%) do (
  cluttered config -i %%n.toml
)
pause