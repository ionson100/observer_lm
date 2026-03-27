#!/bin/bash

dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o ./temp-bin
#сжатие бинарника
#dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true -p:TrimMode=link -o ./temp-bin
cp ./temp-bin/observerLm ./observerlm-deb/usr/local/bin/
echo "Бинарник создан"
chmod +x ./observerlm-deb/usr/local/bin/observerLm
chmod -R 755 ./observerlm-deb/DEBIAN/
echo "Права установлены"
# Очистка и восстановление
dotnet restore -r linux-x64
# Сборка DEB-пакета

dpkg-deb --build observerlm-deb "observerlm-deb_$(grep '^Version:' observerlm-deb/DEBIAN/control | awk '{print $2}')_amd64.deb"
rm -rf ./temp-bin
rm  ./observerlm-deb/usr/local/bin/observerLm

echo "Пакет создан"
exit 0
