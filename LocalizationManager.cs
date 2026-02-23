using System;
using System.Collections.Generic;
using Mysqlx.Crud;

namespace SecureNotes
{
    public enum Language
    {
        Ukrainian,
        English,
        Polish,
        German,
        French,
        Spanish,
        Italian,
        Portuguese,
        Czech,
        Turkish,
        Japanese,
        Chinese,
        Korean,
        Dutch,
        Swedish
    }

    public static class LocalizationManager
    {
        private static Language _currentLanguage = Language.Ukrainian;
        private static readonly Language[] AllLanguages = (Language[])Enum.GetValues(typeof(Language));


        public static event Action LanguageChanged;

        public static Language CurrentLanguage
        {
            get => _currentLanguage;
            set
            {
                if (_currentLanguage != value)
                {
                    _currentLanguage = value;
                    LanguageChanged?.Invoke();
                }
            }
        }

        private static readonly Dictionary<string, Dictionary<Language, string>> Translations =
            new Dictionary<string, Dictionary<Language, string>>
            {
                ["app_title"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "SecureNotes",
                    [Language.English] = "SecureNotes",
                    [Language.Polish] = "SecureNotes",
                    [Language.German] = "SecureNotes",
                    [Language.French] = "SecureNotes",
                    [Language.Spanish] = "SecureNotes",
                    [Language.Italian] = "SecureNotes",
                    [Language.Portuguese] = "SecureNotes",
                    [Language.Czech] = "SecureNotes",
                    [Language.Turkish] = "SecureNotes",
                    [Language.Japanese] = "SecureNotes",
                    [Language.Chinese] = "SecureNotes",
                    [Language.Korean] = "SecureNotes",
                    [Language.Dutch] = "SecureNotes",
                    [Language.Swedish] = "SecureNotes"
                },
                ["notes"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Нотатки",
                    [Language.English] = "Notes",
                    [Language.Polish] = "Notatki",
                    [Language.German] = "Notizen",
                    [Language.French] = "Notes",
                    [Language.Spanish] = "Notas",
                    [Language.Italian] = "Note",
                    [Language.Portuguese] = "Notas",
                    [Language.Czech] = "Poznamky",
                    [Language.Turkish] = "Notlar",
                    [Language.Japanese] = "メモ",
                    [Language.Chinese] = "笔记",
                    [Language.Korean] = "노트",
                    [Language.Dutch] = "Notities",
                    [Language.Swedish] = "Anteckningar"
                },
                ["passwords"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Паролі",
                    [Language.English] = "Passwords",
                    [Language.Polish] = "Hasla",
                    [Language.German] = "Passworter",
                    [Language.French] = "Mots de passe",
                    [Language.Spanish] = "Contrasenas",
                    [Language.Italian] = "Password",
                    [Language.Portuguese] = "Senhas",
                    [Language.Czech] = "Hesla",
                    [Language.Turkish] = "Sifreler",
                    [Language.Japanese] = "パスワード",
                    [Language.Chinese] = "密码",
                    [Language.Korean] = "비밀번호",
                    [Language.Dutch] = "Wachtwoorden",
                    [Language.Swedish] = "Losenord"
                },
                ["groups"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Групи",
                    [Language.English] = "Groups",
                    [Language.Polish] = "Grupy",
                    [Language.German] = "Gruppen",
                    [Language.French] = "Groupes",
                    [Language.Spanish] = "Grupos",
                    [Language.Italian] = "Gruppi",
                    [Language.Portuguese] = "Grupos",
                    [Language.Czech] = "Skupiny",
                    [Language.Turkish] = "Gruplar",
                    [Language.Japanese] = "グループ",
                    [Language.Chinese] = "群组",
                    [Language.Korean] = "그룹",
                    [Language.Dutch] = "Groepen",
                    [Language.Swedish] = "Grupper"
                },
                ["add_note"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "+ Додати нотатку",
                    [Language.English] = "+ Add Note",
                    [Language.Polish] = "+ Dodaj notatke",
                    [Language.German] = "+ Notiz hinzufugen",
                    [Language.French] = "+ Ajouter une note",
                    [Language.Spanish] = "+ Anadir nota",
                    [Language.Italian] = "+ Aggiungi nota",
                    [Language.Portuguese] = "+ Adicionar nota",
                    [Language.Czech] = "+ Pridat poznamku",
                    [Language.Turkish] = "+ Not ekle",
                    [Language.Japanese] = "+ メモを追加",
                    [Language.Chinese] = "+ 添加笔记",
                    [Language.Korean] = "+ 노트 추가",
                    [Language.Dutch] = "+ Notitie toevoegen",
                    [Language.Swedish] = "+ Lagg till anteckning"
                },
                ["title"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Заголовок",
                    [Language.English] = "Title",
                    [Language.Polish] = "Tytul",
                    [Language.German] = "Titel",
                    [Language.French] = "Titre",
                    [Language.Spanish] = "Titulo",
                    [Language.Italian] = "Titolo",
                    [Language.Portuguese] = "Titulo",
                    [Language.Czech] = "Nazev",
                    [Language.Turkish] = "Baslik",
                    [Language.Japanese] = "タイトル",
                    [Language.Chinese] = "标题",
                    [Language.Korean] = "제목",
                    [Language.Dutch] = "Titel",
                    [Language.Swedish] = "Titel"
                },
                ["content"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Зміст",
                    [Language.English] = "Content",
                    [Language.Polish] = "Tresc",
                    [Language.German] = "Inhalt",
                    [Language.French] = "Contenu",
                    [Language.Spanish] = "Contenido",
                    [Language.Italian] = "Contenuto",
                    [Language.Portuguese] = "Conteudo",
                    [Language.Czech] = "Obsah",
                    [Language.Turkish] = "Icerik",
                    [Language.Japanese] = "内容",
                    [Language.Chinese] = "内容",
                    [Language.Korean] = "내용",
                    [Language.Dutch] = "Inhoud",
                    [Language.Swedish] = "Innehall"
                },
                ["save"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Зберегти",
                    [Language.English] = "Save",
                    [Language.Polish] = "Zapisz",
                    [Language.German] = "Speichern",
                    [Language.French] = "Enregistrer",
                    [Language.Spanish] = "Guardar",
                    [Language.Italian] = "Salva",
                    [Language.Portuguese] = "Salvar",
                    [Language.Czech] = "Ulozit",
                    [Language.Turkish] = "Kaydet",
                    [Language.Japanese] = "保存",
                    [Language.Chinese] = "保存",
                    [Language.Korean] = "저장",
                    [Language.Dutch] = "Opslaan",
                    [Language.Swedish] = "Spara"
                },
                ["create"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Створити",
                    [Language.English] = "Create",
                    [Language.Polish] = "Utworz",
                    [Language.German] = "Erstellen",
                    [Language.French] = "Creer",
                    [Language.Spanish] = "Crear",
                    [Language.Italian] = "Crea",
                    [Language.Portuguese] = "Criar",
                    [Language.Czech] = "Vytvorit",
                    [Language.Turkish] = "Olustur",
                    [Language.Japanese] = "作成",
                    [Language.Chinese] = "创建",
                    [Language.Korean] = "만들기",
                    [Language.Dutch] = "Maken",
                    [Language.Swedish] = "Skapa"
                },
                ["create_note"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Створити нотатку",
                    [Language.English] = "Create Note",
                    [Language.Polish] = "Utworz notatke",
                    [Language.German] = "Notiz erstellen",
                    [Language.French] = "Creer une note",
                    [Language.Spanish] = "Crear nota",
                    [Language.Italian] = "Crea nota",
                    [Language.Portuguese] = "Criar nota",
                    [Language.Czech] = "Vytvorit poznamku",
                    [Language.Turkish] = "Not olustur",
                    [Language.Japanese] = "メモを作成",
                    [Language.Chinese] = "创建笔记",
                    [Language.Korean] = "노트 만들기",
                    [Language.Dutch] = "Notitie maken",
                    [Language.Swedish] = "Skapa anteckning"
                },
                ["edit_note"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Редагувати нотатку",
                    [Language.English] = "Edit Note",
                    [Language.Polish] = "Edytuj notatke",
                    [Language.German] = "Notiz bearbeiten",
                    [Language.French] = "Modifier la note",
                    [Language.Spanish] = "Editar nota",
                    [Language.Italian] = "Modifica nota",
                    [Language.Portuguese] = "Editar nota",
                    [Language.Czech] = "Upravit poznamku",
                    [Language.Turkish] = "Notu duzenle",
                    [Language.Japanese] = "メモを編集",
                    [Language.Chinese] = "编辑笔记",
                    [Language.Korean] = "노트 편집",
                    [Language.Dutch] = "Notitie bewerken",
                    [Language.Swedish] = "Redigera anteckning"
                },
                ["edit"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Редагувати",
                    [Language.English] = "Edit",
                    [Language.Polish] = "Edytuj",
                    [Language.German] = "Bearbeiten",
                    [Language.French] = "Modifier",
                    [Language.Spanish] = "Editar",
                    [Language.Italian] = "Modifica",
                    [Language.Portuguese] = "Editar",
                    [Language.Czech] = "Upravit",
                    [Language.Turkish] = "Duzenle",
                    [Language.Japanese] = "編集",
                    [Language.Chinese] = "编辑",
                    [Language.Korean] = "편집",
                    [Language.Dutch] = "Bewerken",
                    [Language.Swedish] = "Redigera"
                },
                ["delete"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Видалити",
                    [Language.English] = "Delete",
                    [Language.Polish] = "Usun",
                    [Language.German] = "Loschen",
                    [Language.French] = "Supprimer",
                    [Language.Spanish] = "Eliminar",
                    [Language.Italian] = "Elimina",
                    [Language.Portuguese] = "Excluir",
                    [Language.Czech] = "Smazat",
                    [Language.Turkish] = "Sil",
                    [Language.Japanese] = "削除",
                    [Language.Chinese] = "删除",
                    [Language.Korean] = "삭제",
                    [Language.Dutch] = "Verwijderen",
                    [Language.Swedish] = "Ta bort"
                },
                ["copy"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Копіювати",
                    [Language.English] = "Copy",
                    [Language.Polish] = "Kopiuj",
                    [Language.German] = "Kopieren",
                    [Language.French] = "Copier",
                    [Language.Spanish] = "Copiar",
                    [Language.Italian] = "Copia",
                    [Language.Portuguese] = "Copiar",
                    [Language.Czech] = "Kopirovat",
                    [Language.Turkish] = "Kopyala",
                    [Language.Japanese] = "コピー",
                    [Language.Chinese] = "复制",
                    [Language.Korean] = "복사",
                    [Language.Dutch] = "Kopieren",
                    [Language.Swedish] = "Kopiera"
                },
                ["warning"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Увага",
                    [Language.English] = "Warning",
                    [Language.Polish] = "Uwaga",
                    [Language.German] = "Warnung",
                    [Language.French] = "Attention",
                    [Language.Spanish] = "Advertencia",
                    [Language.Italian] = "Attenzione",
                    [Language.Portuguese] = "Aviso",
                    [Language.Czech] = "Upozorneni",
                    [Language.Turkish] = "Uyari",
                    [Language.Japanese] = "警告",
                    [Language.Chinese] = "警告",
                    [Language.Korean] = "경고",
                    [Language.Dutch] = "Waarschuwing",
                    [Language.Swedish] = "Varning"
                },
                ["search"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Пошук...",
                    [Language.English] = "Search...",
                    [Language.Polish] = "Szukaj...",
                    [Language.German] = "Suchen...",
                    [Language.French] = "Rechercher...",
                    [Language.Spanish] = "Buscar...",
                    [Language.Italian] = "Cerca...",
                    [Language.Portuguese] = "Pesquisar...",
                    [Language.Czech] = "Hledat...",
                    [Language.Turkish] = "Ara...",
                    [Language.Japanese] = "検索...",
                    [Language.Chinese] = "搜索...",
                    [Language.Korean] = "검색...",
                    [Language.Dutch] = "Zoeken...",
                    [Language.Swedish] = "Sok..."
                },
                ["settings"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Налаштування",
                    [Language.English] = "Settings",
                    [Language.Polish] = "Ustawienia",
                    [Language.German] = "Einstellungen",
                    [Language.French] = "Parametres",
                    [Language.Spanish] = "Configuracion",
                    [Language.Italian] = "Impostazioni",
                    [Language.Portuguese] = "Configuracoes",
                    [Language.Czech] = "Nastaveni",
                    [Language.Turkish] = "Ayarlar",
                    [Language.Japanese] = "設定",
                    [Language.Chinese] = "设置",
                    [Language.Korean] = "설정",
                    [Language.Dutch] = "Instellingen",
                    [Language.Swedish] = "Installningar"
                },
                ["logout_confirm"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Ви впевнені, що хочете вийти з акаунту?",
                    [Language.English] = "Are you sure you want to log out?",
                    [Language.Polish] = "Czy na pewno chcesz sie wylogowac?",
                    [Language.German] = "Mochten Sie sich wirklich abmelden?",
                    [Language.French] = "Voulez-vous vraiment vous deconnecter ?",
                    [Language.Spanish] = "Seguro que quieres cerrar sesion?",
                    [Language.Italian] = "Sei sicuro di voler uscire?",
                    [Language.Portuguese] = "Tem certeza de que deseja sair?",
                    [Language.Czech] = "Opravdu se chcete odhlasit?",
                    [Language.Turkish] = "Cikis yapmak istediginize emin misiniz?",
                    [Language.Japanese] = "本当にログアウトしますか？",
                    [Language.Chinese] = "确定要退出登录吗？",
                    [Language.Korean] = "로그아웃하시겠습니까?",
                    [Language.Dutch] = "Weet je zeker dat je wilt uitloggen?",
                    [Language.Swedish] = "Ar du saker pa att du vill logga ut?"
                },
                ["themes"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Теми",
                    [Language.English] = "Themes",
                    [Language.Polish] = "Motywy",
                    [Language.German] = "Themen",
                    [Language.French] = "Themes",
                    [Language.Spanish] = "Temas",
                    [Language.Italian] = "Temi",
                    [Language.Portuguese] = "Temas",
                    [Language.Czech] = "Motivy",
                    [Language.Turkish] = "Temalar",
                    [Language.Japanese] = "テーマ",
                    [Language.Chinese] = "主题",
                    [Language.Korean] = "테마",
                    [Language.Dutch] = "Thema's",
                    [Language.Swedish] = "Teman"
                },
                ["color"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Колір",
                    [Language.English] = "Color",
                    [Language.Polish] = "Kolor",
                    [Language.German] = "Farbe",
                    [Language.French] = "Couleur",
                    [Language.Spanish] = "Color",
                    [Language.Italian] = "Colore",
                    [Language.Portuguese] = "Cor",
                    [Language.Czech] = "Barva",
                    [Language.Turkish] = "Renk",
                    [Language.Japanese] = "色",
                    [Language.Chinese] = "颜色",
                    [Language.Korean] = "색상",
                    [Language.Dutch] = "Kleur",
                    [Language.Swedish] = "Farg"
                },
                ["tags"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Теги (через ;)",
                    [Language.English] = "Tags (separated by ;)",
                    [Language.Polish] = "Tagi (oddziel ;)",
                    [Language.German] = "Tags (getrennt durch ;)",
                    [Language.French] = "Tags (separes par ;)",
                    [Language.Spanish] = "Etiquetas (separadas por ;)",
                    [Language.Italian] = "Tag (separati da ;)",
                    [Language.Portuguese] = "Tags (separadas por ;)",
                    [Language.Czech] = "Stitky (oddelene ;)",
                    [Language.Turkish] = "Etiketler (; ile ayirin)",
                    [Language.Japanese] = "タグ（;で区切る）",
                    [Language.Chinese] = "标签（用;分隔）",
                    [Language.Korean] = "태그 (;로 구분)",
                    [Language.Dutch] = "Tags (gescheiden door ;)",
                    [Language.Swedish] = "Taggar (separerade med ;)"
                },
                ["type"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Тип",
                    [Language.English] = "Type",
                    [Language.Polish] = "Typ",
                    [Language.German] = "Typ",
                    [Language.French] = "Type",
                    [Language.Spanish] = "Tipo",
                    [Language.Italian] = "Tipo",
                    [Language.Portuguese] = "Tipo",
                    [Language.Czech] = "Typ",
                    [Language.Turkish] = "Tur",
                    [Language.Japanese] = "タイプ",
                    [Language.Chinese] = "类型",
                    [Language.Korean] = "유형",
                    [Language.Dutch] = "Type",
                    [Language.Swedish] = "Typ"
                },
                ["regular"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Звичайна",
                    [Language.English] = "Regular",
                    [Language.Polish] = "Zwykla",
                    [Language.German] = "Normal",
                    [Language.French] = "Normale",
                    [Language.Spanish] = "Normal",
                    [Language.Italian] = "Normale",
                    [Language.Portuguese] = "Normal",
                    [Language.Czech] = "Bezna",
                    [Language.Turkish] = "Normal",
                    [Language.Japanese] = "通常",
                    [Language.Chinese] = "普通",
                    [Language.Korean] = "일반",
                    [Language.Dutch] = "Normaal",
                    [Language.Swedish] = "Vanlig"
                },
                ["password"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Пароль",
                    [Language.English] = "Password",
                    [Language.Polish] = "Haslo",
                    [Language.German] = "Passwort",
                    [Language.French] = "Mot de passe",
                    [Language.Spanish] = "Contrasena",
                    [Language.Italian] = "Password",
                    [Language.Portuguese] = "Senha",
                    [Language.Czech] = "Heslo",
                    [Language.Turkish] = "Sifre",
                    [Language.Japanese] = "パスワード",
                    [Language.Chinese] = "密码",
                    [Language.Korean] = "비밀번호",
                    [Language.Dutch] = "Wachtwoord",
                    [Language.Swedish] = "Losenord"
                },
                ["shared_note"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Спільна нотатка",
                    [Language.English] = "Shared note",
                    [Language.Polish] = "Udostepniona notatka",
                    [Language.German] = "Geteilte Notiz",
                    [Language.French] = "Note partagee",
                    [Language.Spanish] = "Nota compartida",
                    [Language.Italian] = "Nota condivisa",
                    [Language.Portuguese] = "Nota compartilhada",
                    [Language.Czech] = "Sdilena poznamka",
                    [Language.Turkish] = "Paylasilan not",
                    [Language.Japanese] = "共有メモ",
                    [Language.Chinese] = "共享笔记",
                    [Language.Korean] = "공유 노트",
                    [Language.Dutch] = "Gedeelde notitie",
                    [Language.Swedish] = "Delad anteckning"
                },
                ["group"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Група",
                    [Language.English] = "Group",
                    [Language.Polish] = "Grupa",
                    [Language.German] = "Gruppe",
                    [Language.French] = "Groupe",
                    [Language.Spanish] = "Grupo",
                    [Language.Italian] = "Gruppo",
                    [Language.Portuguese] = "Grupo",
                    [Language.Czech] = "Skupina",
                    [Language.Turkish] = "Grup",
                    [Language.Japanese] = "グループ",
                    [Language.Chinese] = "群组",
                    [Language.Korean] = "그룹",
                    [Language.Dutch] = "Groep",
                    [Language.Swedish] = "Grupp"
                },
                ["language"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Мова",
                    [Language.English] = "Language",
                    [Language.Polish] = "Jezyk",
                    [Language.German] = "Sprache",
                    [Language.French] = "Langue",
                    [Language.Spanish] = "Idioma",
                    [Language.Italian] = "Lingua",
                    [Language.Portuguese] = "Idioma",
                    [Language.Czech] = "Jazyk",
                    [Language.Turkish] = "Dil",
                    [Language.Japanese] = "言語",
                    [Language.Chinese] = "语言",
                    [Language.Korean] = "언어",
                    [Language.Dutch] = "Taal",
                    [Language.Swedish] = "Sprak"
                },
                ["my_notes"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Мої нотатки",
                    [Language.English] = "My Notes",
                    [Language.Polish] = "Moje notatki",
                    [Language.German] = "Meine Notizen",
                    [Language.French] = "Mes notes",
                    [Language.Spanish] = "Mis notas",
                    [Language.Italian] = "Le mie note",
                    [Language.Portuguese] = "Minhas notas",
                    [Language.Czech] = "Moje poznamky",
                    [Language.Turkish] = "Notlarim",
                    [Language.Japanese] = "マイメモ",
                    [Language.Chinese] = "我的笔记",
                    [Language.Korean] = "내 노트",
                    [Language.Dutch] = "Mijn notities",
                    [Language.Swedish] = "Mina anteckningar"
                },
                ["all_tags"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "(усі теги)",
                    [Language.English] = "(all tags)",
                    [Language.Polish] = "(wszystkie tagi)",
                    [Language.German] = "(alle Tags)",
                    [Language.French] = "(tous les tags)",
                    [Language.Spanish] = "(todas las etiquetas)",
                    [Language.Italian] = "(tutti i tag)",
                    [Language.Portuguese] = "(todas as tags)",
                    [Language.Czech] = "(vsechny stitky)",
                    [Language.Turkish] = "(tum etiketler)",
                    [Language.Japanese] = "（すべてのタグ）",
                    [Language.Chinese] = "（所有标签）",
                    [Language.Korean] = "(모든 태그)",
                    [Language.Dutch] = "(alle tags)",
                    [Language.Swedish] = "(alla taggar)"
                },
                ["attach_file"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Файл",
                    [Language.English] = "File",
                    [Language.Polish] = "Plik",
                    [Language.German] = "Datei",
                    [Language.French] = "Fichier",
                    [Language.Spanish] = "Archivo",
                    [Language.Italian] = "File",
                    [Language.Portuguese] = "Arquivo",
                    [Language.Czech] = "Soubor",
                    [Language.Turkish] = "Dosya",
                    [Language.Japanese] = "ファイル",
                    [Language.Chinese] = "文件",
                    [Language.Korean] = "파일",
                    [Language.Dutch] = "Bestand",
                    [Language.Swedish] = "Fil"
                },
                ["size"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Розмір:",
                    [Language.English] = "Size:",
                    [Language.Polish] = "Rozmiar:",
                    [Language.German] = "Grosse:",
                    [Language.French] = "Taille:",
                    [Language.Spanish] = "Tamano:",
                    [Language.Italian] = "Dimensione:",
                    [Language.Portuguese] = "Tamanho:",
                    [Language.Czech] = "Velikost:",
                    [Language.Turkish] = "Boyut:",
                    [Language.Japanese] = "サイズ：",
                    [Language.Chinese] = "大小：",
                    [Language.Korean] = "크기:",
                    [Language.Dutch] = "Grootte:",
                    [Language.Swedish] = "Storlek:"
                },
                ["cancel"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Скасувати",
                    [Language.English] = "Cancel",
                    [Language.Polish] = "Anuluj",
                    [Language.German] = "Abbrechen",
                    [Language.French] = "Annuler",
                    [Language.Spanish] = "Cancelar",
                    [Language.Italian] = "Annulla",
                    [Language.Portuguese] = "Cancelar",
                    [Language.Czech] = "Zrusit",
                    [Language.Turkish] = "Iptal",
                    [Language.Japanese] = "キャンセル",
                    [Language.Chinese] = "取消",
                    [Language.Korean] = "취소",
                    [Language.Dutch] = "Annuleren",
                    [Language.Swedish] = "Avbryt"
                },
                ["login"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Логін",
                    [Language.English] = "Username",
                    [Language.Polish] = "Login",
                    [Language.German] = "Benutzername",
                    [Language.French] = "Identifiant",
                    [Language.Spanish] = "Usuario",
                    [Language.Italian] = "Nome utente",
                    [Language.Portuguese] = "Usuario",
                    [Language.Czech] = "Uzivatelske jmeno",
                    [Language.Turkish] = "Kullanici adi",
                    [Language.Japanese] = "ユーザー名",
                    [Language.Chinese] = "用户名",
                    [Language.Korean] = "사용자 이름",
                    [Language.Dutch] = "Gebruikersnaam",
                    [Language.Swedish] = "Anvandarnamn"
                },
                ["sign_in"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Увійти",
                    [Language.English] = "Sign In",
                    [Language.Polish] = "Zaloguj sie",
                    [Language.German] = "Anmelden",
                    [Language.French] = "Se connecter",
                    [Language.Spanish] = "Iniciar sesion",
                    [Language.Italian] = "Accedi",
                    [Language.Portuguese] = "Entrar",
                    [Language.Czech] = "Prihlasit se",
                    [Language.Turkish] = "Giris yap",
                    [Language.Japanese] = "サインイン",
                    [Language.Chinese] = "登录",
                    [Language.Korean] = "로그인",
                    [Language.Dutch] = "Inloggen",
                    [Language.Swedish] = "Logga in"
                },
                ["create_account"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Створити акаунт",
                    [Language.English] = "Create Account",
                    [Language.Polish] = "Utworz konto",
                    [Language.German] = "Konto erstellen",
                    [Language.French] = "Creer un compte",
                    [Language.Spanish] = "Crear cuenta",
                    [Language.Italian] = "Crea account",
                    [Language.Portuguese] = "Criar conta",
                    [Language.Czech] = "Vytvorit ucet",
                    [Language.Turkish] = "Hesap olustur",
                    [Language.Japanese] = "アカウント作成",
                    [Language.Chinese] = "创建账户",
                    [Language.Korean] = "계정 만들기",
                    [Language.Dutch] = "Account maken",
                    [Language.Swedish] = "Skapa konto"
                },
                ["secure_storage"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Безпечне зберігання нотаток та паролів",
                    [Language.English] = "Secure storage for notes and passwords",
                    [Language.Polish] = "Bezpieczne przechowywanie notatek i hasel",
                    [Language.German] = "Sichere Speicherung von Notizen und Passwortern",
                    [Language.French] = "Stockage securise pour notes et mots de passe",
                    [Language.Spanish] = "Almacenamiento seguro para notas y contrasenas",
                    [Language.Italian] = "Archiviazione sicura per note e password",
                    [Language.Portuguese] = "Armazenamento seguro para notas e senhas",
                    [Language.Czech] = "Bezpecne uloziste pro poznamky a hesla",
                    [Language.Turkish] = "Notlar ve sifreler icin guvenli depolama",
                    [Language.Japanese] = "メモとパスワードの安全な保存",
                    [Language.Chinese] = "安全存储笔记和密码",
                    [Language.Korean] = "노트와 비밀번호의 안전한 저장",
                    [Language.Dutch] = "Veilige opslag voor notities en wachtwoorden",
                    [Language.Swedish] = "Saker lagring for anteckningar och losenord"
                },
                ["change_account"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Змінити акаунт",
                    [Language.English] = "Switch Account",
                    [Language.Polish] = "Zmien konto",
                    [Language.German] = "Konto wechseln",
                    [Language.French] = "Changer de compte",
                    [Language.Spanish] = "Cambiar cuenta",
                    [Language.Italian] = "Cambia account",
                    [Language.Portuguese] = "Trocar conta",
                    [Language.Czech] = "Zmenit ucet",
                    [Language.Turkish] = "Hesap degistir",
                    [Language.Japanese] = "アカウント切替",
                    [Language.Chinese] = "切换账户",
                    [Language.Korean] = "계정 전환",
                    [Language.Dutch] = "Account wisselen",
                    [Language.Swedish] = "Byt konto"
                },
                ["change_password"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Змінити пароль",
                    [Language.English] = "Change Password",
                    [Language.Polish] = "Zmien haslo",
                    [Language.German] = "Passwort andern",
                    [Language.French] = "Changer le mot de passe",
                    [Language.Spanish] = "Cambiar contrasena",
                    [Language.Italian] = "Cambia password",
                    [Language.Portuguese] = "Alterar senha",
                    [Language.Czech] = "Zmenit heslo",
                    [Language.Turkish] = "Sifre degistir",
                    [Language.Japanese] = "パスワード変更",
                    [Language.Chinese] = "更改密码",
                    [Language.Korean] = "비밀번호 변경",
                    [Language.Dutch] = "Wachtwoord wijzigen",
                    [Language.Swedish] = "Andra losenord"
                },
                ["delete_account"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Видалити акаунт",
                    [Language.English] = "Delete Account",
                    [Language.Polish] = "Usun konto",
                    [Language.German] = "Konto loschen",
                    [Language.French] = "Supprimer le compte",
                    [Language.Spanish] = "Eliminar cuenta",
                    [Language.Italian] = "Elimina account",
                    [Language.Portuguese] = "Excluir conta",
                    [Language.Czech] = "Smazat ucet",
                    [Language.Turkish] = "Hesabi sil",
                    [Language.Japanese] = "アカウント削除",
                    [Language.Chinese] = "删除账户",
                    [Language.Korean] = "계정 삭제",
                    [Language.Dutch] = "Account verwijderen",
                    [Language.Swedish] = "Ta bort konto"
                },
                ["copied"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Скопійовано!",
                    [Language.English] = "Copied!",
                    [Language.Polish] = "Skopiowano!",
                    [Language.German] = "Kopiert!",
                    [Language.French] = "Copie!",
                    [Language.Spanish] = "Copiado!",
                    [Language.Italian] = "Copiato!",
                    [Language.Portuguese] = "Copiado!",
                    [Language.Czech] = "Zkopirovano!",
                    [Language.Turkish] = "Kopyalandi!",
                    [Language.Japanese] = "コピーしました！",
                    [Language.Chinese] = "已复制！",
                    [Language.Korean] = "복사됨!",
                    [Language.Dutch] = "Gekopieerd!",
                    [Language.Swedish] = "Kopierat!"
                },
                ["theme_light"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Світла",
                    [Language.English] = "Light",
                    [Language.Polish] = "Jasny",
                    [Language.German] = "Hell",
                    [Language.French] = "Clair",
                    [Language.Spanish] = "Claro",
                    [Language.Italian] = "Chiaro",
                    [Language.Portuguese] = "Claro",
                    [Language.Czech] = "Svetly",
                    [Language.Turkish] = "Acik",
                    [Language.Japanese] = "ライト",
                    [Language.Chinese] = "浅色",
                    [Language.Korean] = "라이트",
                    [Language.Dutch] = "Licht",
                    [Language.Swedish] = "Ljus"
                },
                ["theme_dark"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Темна",
                    [Language.English] = "Dark",
                    [Language.Polish] = "Ciemny",
                    [Language.German] = "Dunkel",
                    [Language.French] = "Sombre",
                    [Language.Spanish] = "Oscuro",
                    [Language.Italian] = "Scuro",
                    [Language.Portuguese] = "Escuro",
                    [Language.Czech] = "Tmavy",
                    [Language.Turkish] = "Karanlik",
                    [Language.Japanese] = "ダーク",
                    [Language.Chinese] = "深色",
                    [Language.Korean] = "다크",
                    [Language.Dutch] = "Donker",
                    [Language.Swedish] = "Mork"
                },
                ["theme_ocean"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Океан",
                    [Language.English] = "Ocean",
                    [Language.Polish] = "Ocean",
                    [Language.German] = "Ozean",
                    [Language.French] = "Ocean",
                    [Language.Spanish] = "Oceano",
                    [Language.Italian] = "Oceano",
                    [Language.Portuguese] = "Oceano",
                    [Language.Czech] = "Ocean",
                    [Language.Turkish] = "Okyanus",
                    [Language.Japanese] = "オーシャン",
                    [Language.Chinese] = "海洋",
                    [Language.Korean] = "오션",
                    [Language.Dutch] = "Oceaan",
                    [Language.Swedish] = "Hav"
                },
                ["theme_forest"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Ліс",
                    [Language.English] = "Forest",
                    [Language.Polish] = "Las",
                    [Language.German] = "Wald",
                    [Language.French] = "Foret",
                    [Language.Spanish] = "Bosque",
                    [Language.Italian] = "Foresta",
                    [Language.Portuguese] = "Floresta",
                    [Language.Czech] = "Les",
                    [Language.Turkish] = "Orman",
                    [Language.Japanese] = "フォレスト",
                    [Language.Chinese] = "森林",
                    [Language.Korean] = "포레스트",
                    [Language.Dutch] = "Bos",
                    [Language.Swedish] = "Skog"
                },
                ["theme_sunset"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Захід сонця",
                    [Language.English] = "Sunset",
                    [Language.Polish] = "Zachod slonca",
                    [Language.German] = "Sonnenuntergang",
                    [Language.French] = "Coucher de soleil",
                    [Language.Spanish] = "Atardecer",
                    [Language.Italian] = "Tramonto",
                    [Language.Portuguese] = "Por do sol",
                    [Language.Czech] = "Zapad slunce",
                    [Language.Turkish] = "Gun batimi",
                    [Language.Japanese] = "サンセット",
                    [Language.Chinese] = "日落",
                    [Language.Korean] = "선셋",
                    [Language.Dutch] = "Zonsondergang",
                    [Language.Swedish] = "Solnedgang"
                },
                ["select_group_first"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Спочатку оберіть групу.",
                    [Language.English] = "Please select a group first."
                },
                ["file_copy_failed"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Не вдалося скопіювати файл: ",
                    [Language.English] = "Failed to copy file: "
                },
                ["select_text_to_highlight"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Виділіть текст для виділення кольором.",
                    [Language.English] = "Select text to apply highlighting."
                },
                ["select_text_to_change_color"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Виділіть текст для зміни кольору.",
                    [Language.English] = "Select text to change text color."
                },
                ["attach_file_dialog_title"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Прикріпити файл",
                    [Language.English] = "Attach file"
                },
                ["file_not_found"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Файл не знайдено.",
                    [Language.English] = "File not found."
                },
                ["open_image_failed"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Не вдалося відкрити зображення: ",
                    [Language.English] = "Failed to open image: "
                },
                ["open_file_failed"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Не вдалося відкрити файл: ",
                    [Language.English] = "Failed to open file: "
                },
                ["decrypt_failed_placeholder"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "(не вдалося розшифрувати)",
                    [Language.English] = "(failed to decrypt)"
                },
                ["enter_title"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Введіть заголовок.",
                    [Language.English] = "Enter a title."
                },
                ["select_group_for_shared_note"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Оберіть групу для спільної нотатки.",
                    [Language.English] = "Select a group for the shared note."
                },
                ["unlock_passwords_tab_first"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Спочатку розблокуйте вкладку Паролі (PIN).",
                    [Language.English] = "Unlock the Passwords tab first (PIN)."
                },
                ["pink"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Рожевий",
                    [Language.English] = "Pink",
                    [Language.Polish] = "Rozowy",
                    [Language.German] = "Rosa",
                    [Language.French] = "Rose",
                    [Language.Spanish] = "Rosa",
                    [Language.Italian] = "Rosa",
                    [Language.Portuguese] = "Rosa",
                    [Language.Czech] = "Ruzova",
                    [Language.Turkish] = "Pembe",
                    [Language.Japanese] = "ピンク",
                    [Language.Chinese] = "粉色",
                    [Language.Korean] = "분홍",
                    [Language.Dutch] = "Roze",
                    [Language.Swedish] = "Rosa"
                },
                ["blue"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Блакитний",
                    [Language.English] = "Blue",
                    [Language.Polish] = "Niebieski",
                    [Language.German] = "Blau",
                    [Language.French] = "Bleu",
                    [Language.Spanish] = "Azul",
                    [Language.Italian] = "Blu",
                    [Language.Portuguese] = "Azul",
                    [Language.Czech] = "Modra",
                    [Language.Turkish] = "Mavi",
                    [Language.Japanese] = "青",
                    [Language.Chinese] = "蓝色",
                    [Language.Korean] = "파랑",
                    [Language.Dutch] = "Blauw",
                    [Language.Swedish] = "Bla"
                },
                ["mint"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "М'ятний",
                    [Language.English] = "Mint",
                    [Language.Polish] = "Mietowy",
                    [Language.German] = "Minze",
                    [Language.French] = "Menthe",
                    [Language.Spanish] = "Menta",
                    [Language.Italian] = "Menta",
                    [Language.Portuguese] = "Menta",
                    [Language.Czech] = "Matova",
                    [Language.Turkish] = "Nane",
                    [Language.Japanese] = "ミント",
                    [Language.Chinese] = "薄荷",
                    [Language.Korean] = "민트",
                    [Language.Dutch] = "Mint",
                    [Language.Swedish] = "Mint"
                },
                ["peach"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Персиковий",
                    [Language.English] = "Peach",
                    [Language.Polish] = "Brzoskwiniowy",
                    [Language.German] = "Pfirsich",
                    [Language.French] = "Peche",
                    [Language.Spanish] = "Melocoton",
                    [Language.Italian] = "Pesca",
                    [Language.Portuguese] = "Pessego",
                    [Language.Czech] = "Broskvova",
                    [Language.Turkish] = "Seftali",
                    [Language.Japanese] = "ピーチ",
                    [Language.Chinese] = "桃色",
                    [Language.Korean] = "복숭아",
                    [Language.Dutch] = "Perzik",
                    [Language.Swedish] = "Persika"
                },
                ["lavender"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Лавандовий",
                    [Language.English] = "Lavender",
                    [Language.Polish] = "Lawendowy",
                    [Language.German] = "Lavendel",
                    [Language.French] = "Lavande",
                    [Language.Spanish] = "Lavanda",
                    [Language.Italian] = "Lavanda",
                    [Language.Portuguese] = "Lavanda",
                    [Language.Czech] = "Levandulova",
                    [Language.Turkish] = "Lavanta",
                    [Language.Japanese] = "ラベンダー",
                    [Language.Chinese] = "薰衣草",
                    [Language.Korean] = "라벤더",
                    [Language.Dutch] = "Lavendel",
                    [Language.Swedish] = "Lavendel"
                },
                ["yellow"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Жовтий",
                    [Language.English] = "Yellow",
                    [Language.Polish] = "Zolty",
                    [Language.German] = "Gelb",
                    [Language.French] = "Jaune",
                    [Language.Spanish] = "Amarillo",
                    [Language.Italian] = "Giallo",
                    [Language.Portuguese] = "Amarelo",
                    [Language.Czech] = "Zluta",
                    [Language.Turkish] = "Sari",
                    [Language.Japanese] = "黄色",
                    [Language.Chinese] = "黄色",
                    [Language.Korean] = "노랑",
                    [Language.Dutch] = "Geel",
                    [Language.Swedish] = "Gul"
                },
                ["white"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Білий",
                    [Language.English] = "White",
                    [Language.Polish] = "Bialy",
                    [Language.German] = "Weiss",
                    [Language.French] = "Blanc",
                    [Language.Spanish] = "Blanco",
                    [Language.Italian] = "Bianco",
                    [Language.Portuguese] = "Branco",
                    [Language.Czech] = "Bila",
                    [Language.Turkish] = "Beyaz",
                    [Language.Japanese] = "白",
                    [Language.Chinese] = "白色",
                    [Language.Korean] = "흰색",
                    [Language.Dutch] = "Wit",
                    [Language.Swedish] = "Vit"
                },
                ["account"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Акаунт",
                    [Language.English] = "Account",
                    [Language.Polish] = "Konto",
                    [Language.German] = "Konto",
                    [Language.French] = "Compte",
                    [Language.Spanish] = "Cuenta",
                    [Language.Italian] = "Account",
                    [Language.Portuguese] = "Conta",
                    [Language.Czech] = "Ucet",
                    [Language.Turkish] = "Hesap",
                    [Language.Japanese] = "アカウント",
                    [Language.Chinese] = "账户",
                    [Language.Korean] = "계정",
                    [Language.Dutch] = "Account",
                    [Language.Swedish] = "Konto"
                },
                ["theme_settings"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Тема оформлення",
                    [Language.English] = "Theme",
                    [Language.Polish] = "Motyw",
                    [Language.German] = "Theme",
                    [Language.French] = "Theme",
                    [Language.Spanish] = "Tema",
                    [Language.Italian] = "Tema",
                    [Language.Portuguese] = "Tema",
                    [Language.Czech] = "Motiv",
                    [Language.Turkish] = "Tema",
                    [Language.Japanese] = "テーマ",
                    [Language.Chinese] = "主题",
                    [Language.Korean] = "테마",
                    [Language.Dutch] = "Thema",
                    [Language.Swedish] = "Tema"
                },
                ["preview"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Попередній перегляд",
                    [Language.English] = "Preview",
                    [Language.Polish] = "Podglad",
                    [Language.German] = "Vorschau",
                    [Language.French] = "Apercu",
                    [Language.Spanish] = "Vista previa",
                    [Language.Italian] = "Anteprima",
                    [Language.Portuguese] = "Visualizacao",
                    [Language.Czech] = "Nahled",
                    [Language.Turkish] = "Onizleme",
                    [Language.Japanese] = "プレビュー",
                    [Language.Chinese] = "预览",
                    [Language.Korean] = "미리보기",
                    [Language.Dutch] = "Voorbeeld",
                    [Language.Swedish] = "Forhandsgranskning"
                },
                ["sample_text"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Приклад тексту",
                    [Language.English] = "Sample text",
                    [Language.Polish] = "Przykladowy tekst",
                    [Language.German] = "Beispieltext",
                    [Language.French] = "Exemple de texte",
                    [Language.Spanish] = "Texto de ejemplo",
                    [Language.Italian] = "Testo di esempio",
                    [Language.Portuguese] = "Texto de exemplo",
                    [Language.Czech] = "Ukazkovy text",
                    [Language.Turkish] = "Ornek metin",
                    [Language.Japanese] = "サンプルテキスト",
                    [Language.Chinese] = "示例文本",
                    [Language.Korean] = "샘플 텍스트",
                    [Language.Dutch] = "Voorbeeldtekst",
                    [Language.Swedish] = "Exempeltext"
                },
                ["secondary_text"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Другорядний текст",
                    [Language.English] = "Secondary text",
                    [Language.Polish] = "Tekst drugorzedny",
                    [Language.German] = "Sekundartext",
                    [Language.French] = "Texte secondaire",
                    [Language.Spanish] = "Texto secundario",
                    [Language.Italian] = "Testo secondario",
                    [Language.Portuguese] = "Texto secundario",
                    [Language.Czech] = "Sekundarni text",
                    [Language.Turkish] = "Ikincil metin",
                    [Language.Japanese] = "セカンダリテキスト",
                    [Language.Chinese] = "次要文本",
                    [Language.Korean] = "보조 텍스트",
                    [Language.Dutch] = "Secundaire tekst",
                    [Language.Swedish] = "Sekundar text"
                },
                ["button"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Кнопка",
                    [Language.English] = "Button",
                    [Language.Polish] = "Przycisk",
                    [Language.German] = "Taste",
                    [Language.French] = "Bouton",
                    [Language.Spanish] = "Boton",
                    [Language.Italian] = "Pulsante",
                    [Language.Portuguese] = "Botao",
                    [Language.Czech] = "Tlacitko",
                    [Language.Turkish] = "Dügme",
                    [Language.Japanese] = "ボタン",
                    [Language.Chinese] = "按钮",
                    [Language.Korean] = "버튼",
                    [Language.Dutch] = "Knop",
                    [Language.Swedish] = "Knapp"
                },
                ["copy_code"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Копіювати код",
                    [Language.English] = "Copy code",
                    [Language.Polish] = "Kopiuj kod",
                    [Language.German] = "Code kopieren",
                    [Language.French] = "Copier le code",
                    [Language.Spanish] = "Copiar codigo",
                    [Language.Italian] = "Copia codice",
                    [Language.Portuguese] = "Copiar codigo",
                    [Language.Czech] = "Zkopirovat kod",
                    [Language.Turkish] = "Kodu kopyala",
                    [Language.Japanese] = "コードをコピー",
                    [Language.Chinese] = "复制代码",
                    [Language.Korean] = "코드 복사",
                    [Language.Dutch] = "Code kopieren",
                    [Language.Swedish] = "Kopiera kod"
                },
                ["leave_group"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Покинути групу",
                    [Language.English] = "Leave group",
                    [Language.Polish] = "Opusc grupe",
                    [Language.German] = "Gruppe verlassen",
                    [Language.French] = "Quitter le groupe",
                    [Language.Spanish] = "Salir del grupo",
                    [Language.Italian] = "Lascia gruppo",
                    [Language.Portuguese] = "Sair do grupo",
                    [Language.Czech] = "Opustit skupinu",
                    [Language.Turkish] = "Gruptan ayril",
                    [Language.Japanese] = "グループを退出",
                    [Language.Chinese] = "退出群组",
                    [Language.Korean] = "그룹 나가기",
                    [Language.Dutch] = "Groep verlaten",
                    [Language.Swedish] = "Lamna grupp"
                },
                ["delete_group"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Видалити групу",
                    [Language.English] = "Delete group",
                    [Language.Polish] = "Usun grupe",
                    [Language.German] = "Gruppe loschen",
                    [Language.French] = "Supprimer le groupe",
                    [Language.Spanish] = "Eliminar grupo",
                    [Language.Italian] = "Elimina gruppo",
                    [Language.Portuguese] = "Excluir grupo",
                    [Language.Czech] = "Smazat skupinu",
                    [Language.Turkish] = "Grubu sil",
                    [Language.Japanese] = "グループを削除",
                    [Language.Chinese] = "删除群组",
                    [Language.Korean] = "그룹 삭제",
                    [Language.Dutch] = "Groep verwijderen",
                    [Language.Swedish] = "Ta bort grupp"
                },
                ["join_group"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Приєднатися",
                    [Language.English] = "Join",
                    [Language.Polish] = "Dolacz",
                    [Language.German] = "Beitreten",
                    [Language.French] = "Rejoindre",
                    [Language.Spanish] = "Unirse",
                    [Language.Italian] = "Unisciti",
                    [Language.Portuguese] = "Entrar",
                    [Language.Czech] = "Pripojit se",
                    [Language.Turkish] = "Katil",
                    [Language.Japanese] = "参加",
                    [Language.Chinese] = "加入",
                    [Language.Korean] = "가입",
                    [Language.Dutch] = "Deelnemen",
                    [Language.Swedish] = "Ga med"
                },
                ["group_name"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Назва групи",
                    [Language.English] = "Group name",
                    [Language.Polish] = "Nazwa grupy",
                    [Language.German] = "Gruppenname",
                    [Language.French] = "Nom du groupe",
                    [Language.Spanish] = "Nombre del grupo",
                    [Language.Italian] = "Nome gruppo",
                    [Language.Portuguese] = "Nome do grupo",
                    [Language.Czech] = "Nazev skupiny",
                    [Language.Turkish] = "Grup adi",
                    [Language.Japanese] = "グループ名",
                    [Language.Chinese] = "群组名称",
                    [Language.Korean] = "그룹 이름",
                    [Language.Dutch] = "Groepsnaam",
                    [Language.Swedish] = "Gruppnamn"
                },
                ["invite_code"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Код",
                    [Language.English] = "Code",
                    [Language.Polish] = "Kod",
                    [Language.German] = "Code",
                    [Language.French] = "Code",
                    [Language.Spanish] = "Codigo",
                    [Language.Italian] = "Codice",
                    [Language.Portuguese] = "Codigo",
                    [Language.Czech] = "Kod",
                    [Language.Turkish] = "Kod",
                    [Language.Japanese] = "コード",
                    [Language.Chinese] = "代码",
                    [Language.Korean] = "코드",
                    [Language.Dutch] = "Code",
                    [Language.Swedish] = "Kod"
                },
                ["shared_notes"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Спільні нотатки",
                    [Language.English] = "Shared Notes",
                    [Language.Polish] = "Udostepnione notatki",
                    [Language.German] = "Geteilte Notizen",
                    [Language.French] = "Notes partagees",
                    [Language.Spanish] = "Notas compartidas",
                    [Language.Italian] = "Note condivise",
                    [Language.Portuguese] = "Notas compartilhadas",
                    [Language.Czech] = "Sdilene poznamky",
                    [Language.Turkish] = "Paylasilan notlar",
                    [Language.Japanese] = "共有メモ",
                    [Language.Chinese] = "共享笔记",
                    [Language.Korean] = "공유 노트",
                    [Language.Dutch] = "Gedeelde notities",
                    [Language.Swedish] = "Delade anteckningar"
                },
                ["select_group"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Оберіть або створіть групу.",
                    [Language.English] = "Select or create a group.",
                    [Language.Polish] = "Wybierz lub utworz grupe.",
                    [Language.German] = "Wahlen oder erstellen Sie eine Gruppe.",
                    [Language.French] = "Selectionnez ou creez un groupe.",
                    [Language.Spanish] = "Seleccione o cree un grupo.",
                    [Language.Italian] = "Seleziona o crea un gruppo.",
                    [Language.Portuguese] = "Selecione ou crie um grupo.",
                    [Language.Czech] = "Vyberte nebo vytvorte skupinu.",
                    [Language.Turkish] = "Bir grup secin veya olusturun.",
                    [Language.Japanese] = "グループを選択または作成してください。",
                    [Language.Chinese] = "选择或创建群组。",
                    [Language.Korean] = "그룹을 선택하거나 만드세요.",
                    [Language.Dutch] = "Selecteer of maak een groep.",
                    [Language.Swedish] = "Valj eller skapa en grupp."
                },
                ["enter_pin"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Введіть PIN-код",
                    [Language.English] = "Enter PIN",
                    [Language.Polish] = "Wprowadz PIN",
                    [Language.German] = "PIN eingeben",
                    [Language.French] = "Entrez le code PIN",
                    [Language.Spanish] = "Introduzca el PIN",
                    [Language.Italian] = "Inserisci il PIN",
                    [Language.Portuguese] = "Digite o PIN",
                    [Language.Czech] = "Zadejte PIN",
                    [Language.Turkish] = "PIN girin",
                    [Language.Japanese] = "PINを入力",
                    [Language.Chinese] = "输入PIN",
                    [Language.Korean] = "PIN 입력",
                    [Language.Dutch] = "Voer PIN in",
                    [Language.Swedish] = "Ange PIN"
                },
                ["pin_required"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "PIN потрібен для доступу до зашифрованих паролів",
                    [Language.English] = "PIN is required to access encrypted passwords",
                    [Language.Polish] = "PIN jest wymagany do dostepu do zaszyfrowanych hasel",
                    [Language.German] = "PIN erforderlich um auf verschlusselte Passworter zuzugreifen",
                    [Language.French] = "Le PIN est requis pour acceder aux mots de passe chiffres",
                    [Language.Spanish] = "Se requiere PIN para acceder a las contrasenas cifradas",
                    [Language.Italian] = "Il PIN e richiesto per accedere alle password crittografate",
                    [Language.Portuguese] = "PIN e necessario para acessar senhas criptografadas",
                    [Language.Czech] = "PIN je potrebny pro pristup k sifrovanych heslum",
                    [Language.Turkish] = "Sifrelenmis sifrelere erismek icin PIN gereklidir",
                    [Language.Japanese] = "暗号化されたパスワードにアクセスするにはPINが必要です",
                    [Language.Chinese] = "需要PIN才能访问加密的密码",
                    [Language.Korean] = "암호화된 비밀번호에 접근하려면 PIN이 필요합니다",
                    [Language.Dutch] = "PIN is vereist om versleutelde wachtwoorden te openen",
                    [Language.Swedish] = "PIN kravs for att komma at krypterade losenord"
                },
                ["unlock"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Розблокувати",
                    [Language.English] = "Unlock",
                    [Language.Polish] = "Odblokuj",
                    [Language.German] = "Entsperren",
                    [Language.French] = "Deverrouiller",
                    [Language.Spanish] = "Desbloquear",
                    [Language.Italian] = "Sblocca",
                    [Language.Portuguese] = "Desbloquear",
                    [Language.Czech] = "Odemknout",
                    [Language.Turkish] = "Kilidini ac",
                    [Language.Japanese] = "ロック解除",
                    [Language.Chinese] = "解锁",
                    [Language.Korean] = "잠금 해제",
                    [Language.Dutch] = "Ontgrendelen",
                    [Language.Swedish] = "Las upp"
                },
                ["create_pin"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Створити PIN",
                    [Language.English] = "Create PIN",
                    [Language.Polish] = "Utworz PIN",
                    [Language.German] = "PIN erstellen",
                    [Language.French] = "Creer un PIN",
                    [Language.Spanish] = "Crear PIN",
                    [Language.Italian] = "Crea PIN",
                    [Language.Portuguese] = "Criar PIN",
                    [Language.Czech] = "Vytvorit PIN",
                    [Language.Turkish] = "PIN olustur",
                    [Language.Japanese] = "PINを作成",
                    [Language.Chinese] = "创建PIN",
                    [Language.Korean] = "PIN 만들기",
                    [Language.Dutch] = "PIN maken",
                    [Language.Swedish] = "Skapa PIN"
                },
                ["change_pin"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Змінити PIN",
                    [Language.English] = "Change PIN",
                    [Language.Polish] = "Zmien PIN",
                    [Language.German] = "PIN andern",
                    [Language.French] = "Changer le PIN",
                    [Language.Spanish] = "Cambiar PIN",
                    [Language.Italian] = "Cambia PIN",
                    [Language.Portuguese] = "Alterar PIN",
                    [Language.Czech] = "Zmenit PIN",
                    [Language.Turkish] = "PIN degistir",
                    [Language.Japanese] = "PINを変更",
                    [Language.Chinese] = "更改PIN",
                    [Language.Korean] = "PIN 변경",
                    [Language.Dutch] = "PIN wijzigen",
                    [Language.Swedish] = "Andra PIN"
                }
                ,
                ["error"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Помилка",
                    [Language.English] = "Error"
                },
                ["success"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Успіх",
                    [Language.English] = "Success"
                },
                ["confirmation"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Підтвердження",
                    [Language.English] = "Confirmation"
                },
                ["close"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Закрити",
                    [Language.English] = "Close"
                },
                ["create_group"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Створити групу",
                    [Language.English] = "Create Group"
                },
                ["login_subtitle"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Безпечне зберігання нотаток та паролів",
                    [Language.English] = "Secure storage for notes and passwords"
                },
                ["enter_login_and_password"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Введіть логін і пароль.",
                    [Language.English] = "Enter username and password."
                },
                ["password_min_length"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Пароль має бути не менше 4 символів.",
                    [Language.English] = "Password must be at least 4 characters long.",
                    [Language.Polish] = "Hasło musi mieć co najmniej 4 znaki.",
                    [Language.German] = "Das Passwort muss mindestens 4 Zeichen lang sein.",
                    [Language.French] = "Le mot de passe doit contenir au moins 4 caractères.",
                    [Language.Spanish] = "La contraseña debe tener al menos 4 caracteres.",
                    [Language.Italian] = "La password deve contenere almeno 4 caratteri.",
                    [Language.Portuguese] = "A senha deve ter pelo menos 4 caracteres.",
                    [Language.Czech] = "Heslo musí mít alespoň 4 znaky.",
                    [Language.Turkish] = "Şifre en az 4 karakter olmalıdır.",
                    [Language.Japanese] = "パスワードは4文字以上である必要があります。",
                    [Language.Chinese] = "密码长度至少为4个字符。",
                    [Language.Korean] = "비밀번호는 최소 4자 이상이어야 합니다.",
                    [Language.Dutch] = "Het wachtwoord moet minimaal 4 tekens lang zijn.",
                    [Language.Swedish] = "Lösenordet måste vara minst 4 tecken långt."
                },
                ["user_exists_or_db_error"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Користувач вже існує або помилка БД.",
                    [Language.English] = "User already exists or a database error occurred."
                },
                ["user_not_found"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Користувача не знайдено.",
                    [Language.English] = "User not found."
                },
                ["invalid_password"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Невірний пароль.",
                    [Language.English] = "Invalid password.",
                    [Language.Polish] = "Nieprawidłowe hasło.",
                    [Language.German] = "Ungültiges Passwort.",
                    [Language.French] = "Mot de passe invalide.",
                    [Language.Spanish] = "Contraseña no válida.",
                    [Language.Italian] = "Password non valida.",
                    [Language.Portuguese] = "Senha inválida.",
                    [Language.Czech] = "Neplatné heslo.",
                    [Language.Turkish] = "Geçersiz şifre.",
                    [Language.Japanese] = "無効なパスワードです。",
                    [Language.Chinese] = "密码无效。",
                    [Language.Korean] = "잘못된 비밀번호입니다.",
                    [Language.Dutch] = "Ongeldig wachtwoord.",
                    [Language.Swedish] = "Ogiltigt lösenord."
                },
                ["account_deletion"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Видалення акаунту",
                    [Language.English] = "Account deletion",
                    [Language.Polish] = "Usuwanie konta",
                    [Language.German] = "Kontolöschung",
                    [Language.French] = "Suppression du compte",
                    [Language.Spanish] = "Eliminación de la cuenta",
                    [Language.Italian] = "Eliminazione account",
                    [Language.Portuguese] = "Exclusão da conta",
                    [Language.Czech] = "Smazání účtu",
                    [Language.Turkish] = "Hesap silme",
                    [Language.Japanese] = "アカウント削除",
                    [Language.Chinese] = "删除账户",
                    [Language.Korean] = "계정 삭제",
                    [Language.Dutch] = "Account verwijderen",
                    [Language.Swedish] = "Kontoborttagning"
                },
                ["irreversible_delete_warning"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Ця дія незворотна. Всі ваші нотатки та паролі будуть видалені назавжди.",
                    [Language.English] = "This action is irreversible. All your notes and passwords will be permanently deleted.",
                    [Language.Polish] = "Ta operacja jest nieodwracalna. Wszystkie Twoje notatki i hasła zostaną trwale usunięte.",
                    [Language.German] = "Diese Aktion ist unwiderruflich. Alle Ihre Notizen und Passwörter werden dauerhaft gelöscht.",
                    [Language.French] = "Cette action est irréversible. Toutes vos notes et mots de passe seront supprimés définitivement.",
                    [Language.Spanish] = "Esta acción es irreversible. Todas tus notas y contraseñas se eliminarán permanentemente.",
                    [Language.Italian] = "Questa azione è irreversibile. Tutte le tue note e password saranno eliminate definitivamente.",
                    [Language.Portuguese] = "Esta ação é irreversível. Todas as suas notas e senhas serão excluídas permanentemente.",
                    [Language.Czech] = "Tato akce je nevratná. Všechny vaše poznámky a hesla budou trvale smazány.",
                    [Language.Turkish] = "Bu işlem geri alınamaz. Tüm notlarınız ve şifreleriniz kalıcı olarak silinecektir.",
                    [Language.Japanese] = "この操作は元に戻せません。すべてのメモとパスワードが 完全に削除されます。",
                    [Language.Chinese] = "此操作不可撤销。您的所有笔记和密码 将被永久删除。",
                    [Language.Korean] = "이 작업은 되돌릴 수 없습니다. 모든 메모와 비밀번호가 영구적으로 삭제됩니다.",
                    [Language.Dutch] = "Deze actie is onomkeerbaar. Al je notities en wachtwoorden worden permanent verwijderd.",
                    [Language.Swedish] = "Denna åtgärd är oåterkallelig. Alla dina anteckningar och lösenord kommer att raderas permanent."

                },
                ["enter_password_confirm"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Введіть пароль для підтвердження:",
                    [Language.English] = "Enter your password to confirm:",
                    [Language.Polish] = "Wpisz hasło, aby potwierdzić:",
                    [Language.German] = "Geben Sie zur Bestätigung Ihr Passwort ein:",
                    [Language.French] = "Entrez votre mot de passe pour confirmer :",
                    [Language.Spanish] = "Introduce tu contraseña para confirmar:",
                    [Language.Italian] = "Inserisci la password per confermare:",
                    [Language.Portuguese] = "Digite sua senha para confirmar:",
                    [Language.Czech] = "Pro potvrzení zadejte heslo:",
                    [Language.Turkish] = "Onaylamak için şifrenizi girin:",
                    [Language.Japanese] = "確認のためパスワードを入力してください：",
                    [Language.Chinese] = "请输入密码以确认：",
                    [Language.Korean] = "확인을 위해 비밀번호를 입력하세요:",
                    [Language.Dutch] = "Voer je wachtwoord in ter bevestiging:",
                    [Language.Swedish] = "Ange ditt lösenord för att bekräfta:"
                },
                ["are_you_sure_irreversible"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Ви впевнені? Цю дію неможливо скасувати.",
                    [Language.English] = "Are you sure? This action cannot be undone.",
                    [Language.Polish] = "Czy na pewno? Tej operacji nie można cofnąć.",
                    [Language.German] = "Sind Sie sicher? Diese Aktion kann nicht rückgängig gemacht werden.",
                    [Language.French] = "Êtes-vous sûr ? Cette action est irréversible.",
                    [Language.Spanish] = "¿Estás seguro? Esta acción no se puede deshacer.",
                    [Language.Italian] = "Sei sicuro? Questa azione non può essere annullata.",
                    [Language.Portuguese] = "Tem certeza? Esta ação não pode ser desfeita.",
                    [Language.Czech] = "Jste si jisti? Tuto akci nelze vrátit zpět.",
                    [Language.Turkish] = "Emin misiniz? Bu işlem geri alınamaz.",
                    [Language.Japanese] = "本当によろしいですか？この操作は元に戻せません。",
                    [Language.Chinese] = "你确定吗？此操作无法撤销。",
                    [Language.Korean] = "정말로 진행하시겠습니까? 이 작업은 되돌릴 수 없습니다.",
                    [Language.Dutch] = "Weet je het zeker? Deze actie kan niet ongedaan worden gemaakt.",
                    [Language.Swedish] = "Är du säker? Denna åtgärd kan inte ångras."
                },
                ["account_deleted_successfully"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Акаунт успішно видалено.",
                    [Language.English] = "Account deleted successfully.",
                    [Language.Polish] = "Konto zostało pomyślnie usunięte.",
                    [Language.German] = "Konto wurde erfolgreich gelöscht.",
                    [Language.French] = "Le compte a été supprimé avec succès.",
                    [Language.Spanish] = "Cuenta eliminada correctamente.",
                    [Language.Italian] = "Account eliminato con successo.",
                    [Language.Portuguese] = "Conta excluída com sucesso.",
                    [Language.Czech] = "Účet byl úspěšně smazán.",
                    [Language.Turkish] = "Hesap başarıyla silindi.",
                    [Language.Japanese] = "アカウントが正常に削除されました。",
                    [Language.Chinese] = "账户已成功删除。",
                    [Language.Korean] = "계정이 성공적으로 삭제되었습니다.",
                    [Language.Dutch] = "Account is succesvol verwijderd.",
                    [Language.Swedish] = "Kontot har tagits bort."
                },
                ["pin_for_passwords"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "PIN для паролів",
                    [Language.English] = "PIN for passwords"
                },
                ["enter_pin_code"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Введіть PIN-код",
                    [Language.English] = "Enter PIN code"
                },
                ["pin_min_length"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "PIN має бути щонайменше 4 символи.",
                    [Language.English] = "PIN must be at least 4 characters long."
                },
                ["current_pin_invalid"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Невірний поточний PIN.",
                    [Language.English] = "Current PIN is incorrect."
                },
                ["new_pin_prompt"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Введіть новий PIN:",
                    [Language.English] = "Enter new PIN:"
                },
                ["new_pin"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Новий PIN",
                    [Language.English] = "New PIN"
                },
                ["new_pin_min_length"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Новий PIN має бути щонайменше 4 символи.",
                    [Language.English] = "New PIN must be at least 4 characters long."
                },
                ["pin_changed_success"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "PIN успішно змінено!",
                    [Language.English] = "PIN changed successfully!"
                },
                ["pin_change_failed"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Не вдалося змінити PIN: ",
                    [Language.English] = "Failed to change PIN: "
                },
                ["pin_created_success"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "PIN успішно створено!",
                    [Language.English] = "PIN created successfully!"
                },
                ["pin_create_failed"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Не вдалося створити PIN: ",
                    [Language.English] = "Failed to create PIN: "
                },
                ["set_pin_first"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Спочатку встановіть PIN.",
                    [Language.English] = "Set up a PIN first."
                },
                ["pin_invalid"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Невірний PIN.",
                    [Language.English] = "Invalid PIN."
                },
                ["current_password"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Поточний пароль",
                    [Language.English] = "Current password",
                    [Language.Polish] = "Aktualne hasło",
                    [Language.German] = "Aktuelles Passwort",
                    [Language.French] = "Mot de passe actuel",
                    [Language.Spanish] = "Contraseña actual",
                    [Language.Italian] = "Password attuale",
                    [Language.Portuguese] = "Senha atual",
                    [Language.Czech] = "Aktuální heslo",
                    [Language.Turkish] = "Mevcut şifre",
                    [Language.Japanese] = "現在のパスワード",
                    [Language.Chinese] = "当前密码",
                    [Language.Korean] = "현재 비밀번호",
                    [Language.Dutch] = "Huidig wachtwoord",
                    [Language.Swedish] = "Nuvarande lösenord"
                },
                ["new_password"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Новий пароль",
                    [Language.English] = "New password",
                    [Language.Polish] = "Nowe hasło",
                    [Language.German] = "Neues Passwort",
                    [Language.French] = "Nouveau mot de passe",
                    [Language.Spanish] = "Nueva contraseña",
                    [Language.Italian] = "Nuova password",
                    [Language.Portuguese] = "Nova senha",
                    [Language.Czech] = "Nové heslo",
                    [Language.Turkish] = "Yeni şifre",
                    [Language.Japanese] = "新しいパスワード",
                    [Language.Chinese] = "新密码",
                    [Language.Korean] = "새 비밀번호",
                    [Language.Dutch] = "Nieuw wachtwoord",
                    [Language.Swedish] = "Nytt lösenord"
                },
                ["confirm_new_password"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Підтвердіть новий пароль",
                    [Language.English] = "Confirm new password",
                    [Language.Polish] = "Potwierdź nowe hasło",
                    [Language.German] = "Neues Passwort bestätigen",
                    [Language.French] = "Confirmez le nouveau mot de passe",
                    [Language.Spanish] = "Confirmar nueva contraseña",
                    [Language.Italian] = "Conferma nuova password",
                    [Language.Portuguese] = "Confirmar nova senha",
                    [Language.Czech] = "Potvrďte nové heslo",
                    [Language.Turkish] = "Yeni şifreyi onayla",
                    [Language.Japanese] = "新しいパスワードを確認",
                    [Language.Chinese] = "确认新密码",
                    [Language.Korean] = "새 비밀번호 확인",
                    [Language.Dutch] = "Bevestig nieuw wachtwoord",
                    [Language.Swedish] = "Bekräfta nytt lösenord"
                },
                ["fill_required_fields"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Заповніть всі поля.",
                    [Language.English] = "Fill in all required fields.",
                    [Language.Polish] = "Wypełnij wszystkie wymagane pola.",
                    [Language.German] = "Füllen Sie alle erforderlichen Felder aus.",
                    [Language.French] = "Remplissez tous les champs obligatoires.",
                    [Language.Spanish] = "Rellena todos los campos obligatorios.",
                    [Language.Italian] = "Compila tutti i campi obbligatori.",
                    [Language.Portuguese] = "Preencha todos os campos obrigatórios.",
                    [Language.Czech] = "Vyplňte všechna povinná pole.",
                    [Language.Turkish] = "Lütfen tüm zorunlu alanları doldurun.",
                    [Language.Japanese] = "必須項目をすべて入力してください。",
                    [Language.Chinese] = "请填写所有必填字段。",
                    [Language.Korean] = "모든 필수 항목을 입력해 주세요.",
                    [Language.Dutch] = "Vul alle verplichte velden in.",
                    [Language.Swedish] = "Fyll i alla obligatoriska fält."
                },
                ["passwords_do_not_match"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Паролі не збігаються.",
                    [Language.English] = "Passwords do not match.",
                    [Language.Polish] = "Hasła nie są takie same.",
                    [Language.German] = "Die Passwörter stimmen nicht überein.",
                    [Language.French] = "Les mots de passe ne correspondent pas.",
                    [Language.Spanish] = "Las contraseñas no coinciden.",
                    [Language.Italian] = "Le password non corrispondono.",
                    [Language.Portuguese] = "As senhas não coincidem.",
                    [Language.Czech] = "Hesla se neshodují.",
                    [Language.Turkish] = "Şifreler eşleşmiyor.",
                    [Language.Japanese] = "パスワードが一致しません。",
                    [Language.Chinese] = "两次输入的密码不一致。",
                    [Language.Korean] = "비밀번호가 일치하지 않습니다.",
                    [Language.Dutch] = "Wachtwoorden komen niet overeen.",
                    [Language.Swedish] = "Lösenorden matchar inte."
                },
                ["current_password_incorrect"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Невірний поточний пароль.",
                    [Language.English] = "Current password is incorrect.",
                    [Language.Polish] = "Aktualne hasło jest nieprawidłowe.",
                    [Language.German] = "Das aktuelle Passwort ist falsch.",
                    [Language.French] = "Le mot de passe actuel est incorrect.",
                    [Language.Spanish] = "La contraseña actual es incorrecta.",
                    [Language.Italian] = "La password attuale non è corretta.",
                    [Language.Portuguese] = "A senha atual está incorreta.",
                    [Language.Czech] = "Aktuální heslo je nesprávné.",
                    [Language.Turkish] = "Mevcut şifre yanlış.",
                    [Language.Japanese] = "現在のパスワードが正しくありません。",
                    [Language.Chinese] = "当前密码不正确。",
                    [Language.Korean] = "현재 비밀번호가 올바르지 않습니다.",
                    [Language.Dutch] = "Huidig wachtwoord is onjuist.",
                    [Language.Swedish] = "Nuvarande lösenord är felaktigt."
                },
                ["password_changed_success"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Пароль успішно змінено!",
                    [Language.English] = "Password changed successfully!",
                    [Language.Polish] = "Hasło zostało pomyślnie zmienione!",
                    [Language.German] = "Passwort wurde erfolgreich geändert!",
                    [Language.French] = "Le mot de passe a été modifié avec succès !",
                    [Language.Spanish] = "¡La contraseña se cambió correctamente!",
                    [Language.Italian] = "Password cambiata con successo!",
                    [Language.Portuguese] = "Senha alterada com sucesso!",
                    [Language.Czech] = "Heslo bylo úspěšně změněno!",
                    [Language.Turkish] = "Şifre başarıyla değiştirildi!",
                    [Language.Japanese] = "パスワードが正常に変更されました！",
                    [Language.Chinese] = "密码修改成功！",
                    [Language.Korean] = "비밀번호가 성공적으로 변경되었습니다!",
                    [Language.Dutch] = "Wachtwoord is succesvol gewijzigd!",
                    [Language.Swedish] = "Lösenordet har ändrats!"
                },
                ["password_change_failed"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Помилка: ",
                    [Language.English] = "Error: ",
                    [Language.Polish] = "Błąd: ",
                    [Language.German] = "Fehler: ",
                    [Language.French] = "Erreur : ",
                    [Language.Spanish] = "Error: ",
                    [Language.Italian] = "Errore: ",
                    [Language.Portuguese] = "Erro: ",
                    [Language.Czech] = "Chyba: ",
                    [Language.Turkish] = "Hata: ",
                    [Language.Japanese] = "エラー: ",
                    [Language.Chinese] = "错误：",
                    [Language.Korean] = "오류: ",
                    [Language.Dutch] = "Fout: ",
                    [Language.Swedish] = "Fel: "
                },
                ["your_code_not_created"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Ваш код: (ще не створено)",
                    [Language.English] = "Your code: (not created yet)"
                },
                ["optional_group_name"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Назва групи (необов'язково)",
                    [Language.English] = "Group name (optional)"
                },
                ["my_group"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Моя група",
                    [Language.English] = "My group"
                },
                ["group_created_share_code"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Групу створено. Поділися кодом з іншими.",
                    [Language.English] = "Group created. Share the code with others."
                },
                ["enter_invite_code_to_join"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Введіть код запрошення для приєднання",
                    [Language.English] = "Enter invite code to join"
                },
                ["group_invite_code_placeholder"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Код запрошення групи",
                    [Language.English] = "Group invite code"
                },
                ["group_not_found"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Група не знайдена.",
                    [Language.English] = "Group not found."
                },
                ["info"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Інформація",
                    [Language.English] = "Info"
                },
                ["enter_group_name"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Введіть назву групи.",
                    [Language.English] = "Enter a group name."
                },
                ["delete_group_confirm"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Видалити групу \"{0}\"?",
                    [Language.English] = "Delete group \"{0}\"?"
                },
                ["delete_group_irreversible"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Цю дію неможливо скасувати!",
                    [Language.English] = "This action cannot be undone!"
                },
                ["leave_group_confirm"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Покинути групу \"{0}\"?",
                    [Language.English] = "Leave group \"{0}\"?"
                },
                ["enter_group_name"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Введіть назву групи",
                    [Language.English] = "Enter group name",
                    [Language.Polish] = "Wpisz nazwę grupy",
                    [Language.German] = "Gruppennamen eingeben",
                    [Language.French] = "Entrez le nom du groupe",
                    [Language.Spanish] = "Ingrese el nombre del grupo",
                    [Language.Italian] = "Inserisci il nome del gruppo",
                    [Language.Portuguese] = "Digite o nome do grupo",
                    [Language.Czech] = "Zadejte název skupiny",
                    [Language.Turkish] = "Grup adını girin",
                    [Language.Japanese] = "グループ名を入力してください",
                    [Language.Chinese] = "请输入群组名称",
                    [Language.Korean] = "그룹 이름을 입력하세요",
                    [Language.Dutch] = "Voer de groepsnaam in",
                    [Language.Swedish] = "Ange gruppnamn"
                },
                ["file_count"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "файлів",
                    [Language.English] = "files",
                    [Language.Polish] = "plików",
                    [Language.German] = "Dateien",
                    [Language.French] = "fichiers",
                    [Language.Spanish] = "archivos",
                    [Language.Italian] = "file",
                    [Language.Portuguese] = "arquivos",
                    [Language.Czech] = "souborů",
                    [Language.Turkish] = "dosya",
                    [Language.Japanese] = "ファイル",
                    [Language.Chinese] = "个文件",
                    [Language.Korean] = "파일",
                    [Language.Dutch] = "bestanden",
                    [Language.Swedish] = "filer"
                },
                ["light"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Світла",
                    [Language.English] = "Light",
                    [Language.Polish] = "Jasny",
                    [Language.German] = "Hell",
                    [Language.French] = "Clair",
                    [Language.Spanish] = "Claro",
                    [Language.Italian] = "Chiaro",
                    [Language.Portuguese] = "Claro",
                    [Language.Czech] = "Světlé",
                    [Language.Turkish] = "Açık",
                    [Language.Japanese] = "ライト",
                    [Language.Chinese] = "浅色",
                    [Language.Korean] = "라이트",
                    [Language.Dutch] = "Licht",
                    [Language.Swedish] = "Ljus"
                },
                ["dark"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Темна",
                    [Language.English] = "Dark",
                    [Language.Polish] = "Ciemny",
                    [Language.German] = "Dunkel",
                    [Language.French] = "Sombre",
                    [Language.Spanish] = "Oscuro",
                    [Language.Italian] = "Scuro",
                    [Language.Portuguese] = "Escuro",
                    [Language.Czech] = "Tmavé",
                    [Language.Turkish] = "Koyu",
                    [Language.Japanese] = "ダーク",
                    [Language.Chinese] = "深色",
                    [Language.Korean] = "다크",
                    [Language.Dutch] = "Donker",
                    [Language.Swedish] = "Mörk"
                },
                ["ocean"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Океан",
                    [Language.English] = "Ocean",
                    [Language.Polish] = "Ocean",
                    [Language.German] = "Ozean",
                    [Language.French] = "Océan",
                    [Language.Spanish] = "Océano",
                    [Language.Italian] = "Oceano",
                    [Language.Portuguese] = "Oceano",
                    [Language.Czech] = "Oceán",
                    [Language.Turkish] = "Okyanus",
                    [Language.Japanese] = "オーシャン",
                    [Language.Chinese] = "海洋",
                    [Language.Korean] = "오션",
                    [Language.Dutch] = "Oceaan",
                    [Language.Swedish] = "Ocean"
                },
                ["forest"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Ліс",
                    [Language.English] = "Forest",
                    [Language.Polish] = "Las",
                    [Language.German] = "Wald",
                    [Language.French] = "Forêt",
                    [Language.Spanish] = "Bosque",
                    [Language.Italian] = "Foresta",
                    [Language.Portuguese] = "Floresta",
                    [Language.Czech] = "Les",
                    [Language.Turkish] = "Orman",
                    [Language.Japanese] = "フォレスト",
                    [Language.Chinese] = "森林",
                    [Language.Korean] = "포레스트",
                    [Language.Dutch] = "Bos",
                    [Language.Swedish] = "Skog"
                },
                ["sunset"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Захід сонця",
                    [Language.English] = "Sunset",
                    [Language.Polish] = "Zachód słońca",
                    [Language.German] = "Sonnenuntergang",
                    [Language.French] = "Coucher de soleil",
                    [Language.Spanish] = "Atardecer",
                    [Language.Italian] = "Tramonto",
                    [Language.Portuguese] = "Pôr do sol",
                    [Language.Czech] = "Západ slunce",
                    [Language.Turkish] = "Gün batımı",
                    [Language.Japanese] = "サンセット",
                    [Language.Chinese] = "日落",
                    [Language.Korean] = "선셋",
                    [Language.Dutch] = "Zonsondergang",
                    [Language.Swedish] = "Solnedgång"
                },
                ["joined_group_check_shared"] = new Dictionary<Language, string>
                {
                    [Language.Ukrainian] = "Ви приєдналися до групи. Перевірте вкладку «Спільні нотатки».",
                    [Language.English] = "You joined the group. Check the Shared Notes tab."
                },
            };
        static LocalizationManager()
        {
            EnsureTranslationsForAllLanguages();
        }

        private static void EnsureTranslationsForAllLanguages()
        {
            foreach (var pair in Translations)
            {
                var langDict = pair.Value;
                langDict.TryGetValue(Language.English, out var englishText);
                langDict.TryGetValue(Language.Ukrainian, out var ukrainianText);

                var fallback = englishText ?? ukrainianText ?? pair.Key;

                foreach (var language in AllLanguages)
                {
                    if (!langDict.ContainsKey(language))
                    {
                        langDict[language] = fallback;
                    }
                }
            }
        }

        public static string Get(string key)
        {
            if (Translations.TryGetValue(key, out var langDict))
            {
                if (langDict.TryGetValue(_currentLanguage, out var translation))
                {
                    return translation;
                }
                if (langDict.TryGetValue(Language.English, out var englishFallback))
                {
                    return englishFallback;
                }
            }
            return key;
        }

        public static string GetLanguageName(Language lang)
        {
            switch (lang)
            {
                case Language.Ukrainian: return "Українська";
                case Language.English: return "English";
                case Language.Polish: return "Polski";
                case Language.German: return "Deutsch";
                case Language.French: return "Francais";
                case Language.Spanish: return "Espanol";
                case Language.Italian: return "Italiano";
                case Language.Portuguese: return "Portugues";
                case Language.Czech: return "Cestina";
                case Language.Turkish: return "Turkce";
                case Language.Japanese: return "日本語";
                case Language.Chinese: return "中文";
                case Language.Korean: return "한국어";
                case Language.Dutch: return "Nederlands";
                case Language.Swedish: return "Svenska";
                default: return lang.ToString();
            }
        }
    }
}
