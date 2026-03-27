#!/bin/bash

# 1. Настройки (измените под свой проект)
APP_NAME="observerLm"
VERSION="1.0.0"
DEB_FOLDER="observerlm-deb"
OUTPUT_DIR="./dist"

echo "--- Начинаем сборку версии $VERSION ---"
mkdir -p $OUTPUT_DIR

# 2. Сборка для Windows (.exe)
echo "--- Сборка для Windows (win-x64) ---"
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o "$OUTPUT_DIR/win"
# Переименуем для удобства
mv "$OUTPUT_DIR/win/$APP_NAME.exe" "$OUTPUT_DIR/${APP_NAME}_${VERSION}.exe"

# 3. Сборка для Linux (.deb)
echo "--- Сборка для Linux (linux-x64) ---"
# Проверка наличия папки для deb
if [ -d "$DEB_FOLDER" ]; then
    # Собираем бинарники
    dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o "./temp-linux"
    
    # Подготовка структуры deb
    mkdir -p "$DEB_FOLDER/usr/local/bin"
    cp "./temp-linux/$APP_NAME" "$DEB_FOLDER/usr/local/bin/"
    
    # Обновляем версию в control (только если работаем в Linux/WSL, где есть sed)
    if [[ "$OSTYPE" == "linux-gnu"* ]]; then
        sed -i "s/^Version:.*$/Version: $VERSION/" "$DEB_FOLDER/DEBIAN/control"
        chmod +x "$DEB_FOLDER/usr/local/bin/$APP_NAME"
        chmod -R 755 "$DEB_FOLDER/DEBIAN/"
        
        echo "--- Упаковка .deb пакета ---"
        dpkg-deb --build "$DEB_FOLDER" "$OUTPUT_DIR/${APP_NAME}_${VERSION}_amd64.deb"
    else
        echo "Предупреждение: Сборка .deb возможна только в среде Linux (WSL). Пропускаем упаковку."
    fi
    
    # Очистка временных файлов
    rm -rf "./temp-linux"
    rm  ./observerlm-deb/usr/local/bin/observerLm
else
    echo "Ошибка: Папка $DEB_FOLDER не найдена!"
fi

echo "--- Готово! Файлы в папке $OUTPUT_DIR ---"
