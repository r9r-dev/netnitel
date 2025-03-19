echo off
set /p "tag=Enter Version: "
docker build -t netnitel:%tag% .
docker tag netnitel:%tag% docker.lamour.bzh/netnitel:%tag%
docker tag netnitel:%tag% docker.lamour.bzh/netnitel:latest
docker push docker.lamour.bzh/netnitel:%tag%
docker push docker.lamour.bzh/netnitel:latest
pause