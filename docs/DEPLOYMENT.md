# Sunucuya Kurulum (VPS Deployment)

Bu doküman, Asl-Su uygulamasını Docker ile kendi sunucunuzda (root erişimli bir Linux VPS)
sıfırdan ayağa kaldırmak için gereken adımları içerir. Aşağıdaki komutları **kendi
bilgisayarınızdaki bir terminalden SSH ile sunucuya bağlandıktan sonra** çalıştırın —
sunucu şifrenizi/anahtarınızı hiçbir sohbet arayüzüne yapıştırmayın.

## 0. Sunucuya bağlan ve root şifresini değiştir

```
ssh root@<SUNUCU_IP>
passwd
```

Eğer bu şifreyi daha önce bir yerde (mesaj, sohbet, e-posta) paylaştıysanız, mutlaka
değiştirin — paylaşılan şifreler ele geçmiş sayılır.

## 1. Docker ve gerekli araçları kur

```
apt update && apt install -y docker.io docker-compose-plugin git
systemctl enable --now docker
```

## 2. Depoyu indir

Repo özel (private) olduğundan bir GitHub Personal Access Token (PAT) gerekir:
GitHub → Settings → Developer settings → Personal access tokens → Generate new token
(sadece `repo` yetkisiyle). Token'ı **kendi bilgisayarınızda** oluşturup sunucuda şu
şekilde kullanın (token'ı benimle paylaşmanıza gerek yok):

```
git clone https://<GITHUB_KULLANICI_ADINIZ>:<PAT>@github.com/FFoxconn/Asl-Su.git
cd Asl-Su
```

## 3. Ortam değişkenlerini ayarla

```
cp .env.example .env
nano .env
```

En az şunları doldurun:

- `MSSQL_SA_PASSWORD` — veritabanı şifresi (güçlü, rastgele bir değer verin)
- `JWT_SIGNING_KEY` — uzun, rastgele bir metin (giriş token'larını imzalamak için)
- `SEED_ADMIN_EMAIL` / `SEED_ADMIN_PASSWORD` — panele ilk girişte kullanacağınız admin hesabı

`TRENDYOL_*` alanlarını, Trendyol Go'dan (partner.tgomarket.com + developers.tgoapps.com +
çağrı merkezi) aldığınız BaseUrl/AgentName/ExecutorUser bilgileri elinize geçtikçe
doldurabilirsiniz — boş bıraksanız da uygulama çalışır, sadece Trendyol senkronizasyonu
"yapılandırılmadı" der.

## 4. Uygulamayı ayağa kaldır

```
docker compose -f docker-compose.prod.yml up -d --build
```

İlk çalıştırmada veritabanı migration'ları otomatik uygulanır ve `SEED_ADMIN_EMAIL`/
`SEED_ADMIN_PASSWORD` doluysa ilk admin kullanıcı otomatik oluşturulur.

## 5. Güvenlik duvarını aç

```
ufw allow 22/tcp
ufw allow 80/tcp
ufw allow 443/tcp
ufw enable
```

(API konteyneri sadece web/nginx üzerinden `/api/` proxy'siyle erişilebilir; 8080'i
dışarıya açmanıza gerek yok, `docker-compose.prod.yml`'deki `ports: 8080:8080` satırını
dışarıdan erişim istemiyorsanız kaldırabilirsiniz.)

## 6. Kontrol et

```
curl -I http://localhost
docker compose -f docker-compose.prod.yml ps
docker compose -f docker-compose.prod.yml logs -f api
```

Tarayıcıdan `http://<SUNUCU_IP>` adresine gidip giriş ekranını görmelisiniz.

## Güncelleme (yeni kod geldiğinde)

```
cd Asl-Su
git pull
docker compose -f docker-compose.prod.yml up -d --build
```

## Sorun giderme

- **`web` açılıyor ama giriş çalışmıyor**: `docker compose -f docker-compose.prod.yml logs api`
  ile hata mesajına bakın; genelde `.env` içindeki `JWT_SIGNING_KEY` veya
  `MSSQL_SA_PASSWORD` eksik/yanlıştır.
- **SQL Server konteyneri başlamıyor**: `MSSQL_SA_PASSWORD` SQL Server'ın güçlü şifre
  kuralına uymalı (en az 8 karakter, büyük/küçük harf + rakam + özel karakter).
- **Trendyol Go bağlantı testi başarısız**: `docs/TRENDYOL_GO_SETUP.md` dosyasındaki
  "Troubleshooting" bölümüne bakın.
