# Deploy TrollTrack to Google Play (invite-only / testing)

This guide walks you through publishing **TrollTrack** (`com.trolltrack.fishing`) to Google Play so only people you invite can install it. It assumes you have not used Google Play Console before.

**What “invite only” means on Google Play**

- Google does **not** have a single “private app for friends” mode like a web beta link with a password.
- Instead you use **testing tracks**: you add testers by **email** or **Google Group**, and only those accounts can install from the Play Store link.
- The app is still distributed through Google Play (testers use the normal Play Store app). It is **not** listed in public search until you promote a release to **Production**.

The two main options:

| Track | Who can install | Typical use |
|--------|------------------|-------------|
| **Internal testing** | Up to **100** testers | Fastest updates, small team |
| **Closed testing** | Larger lists (emails or Google Group) | Invite-only beta, more testers |

For “invite only” with a modest group, **internal testing** is usually the simplest place to start. You can add **closed testing** later without changing your app binary.

---

## Part 1 — Before you start (accounts and one-time fees)

1. **Google account**  
   Use a Google account you control long-term (e.g. your business or personal Gmail). You will tie the Play developer account to it.

2. **Google Play developer registration**  
   - Go to [Google Play Console](https://play.google.com/console).  
   - Pay the **one-time** registration fee (check current pricing on Google’s site; it has historically been around USD $25).  
   - Complete identity verification if Google asks (name, address, sometimes ID). This can take a short time.

3. **Free vs paid — read this before you click “Create app”**  
   While creating the app, Play may show:

   > *You can edit this on the App pricing page up until you publish your app. Once you've published, you can't change a free app to paid.*

   **What that means in practice**

   - **Until the app is “published”** in the sense Play uses for pricing (your first **production** release that makes the store listing live for end users—confirm exact wording in Console help for your case), you can usually open **Grow → Monetization setup** (or **App pricing**) and change how the **download** is priced (free vs paid).
   - **After** you have **published** a **free** app (users install it with no upfront Play Store purchase), Google **does not allow** turning that **same app listing** into a **paid download** app. The listing stays a free-to-install product; you would use **in-app purchases**, **subscriptions**, or a **separate new app** with a new package name if you need an upfront paid SKU.
   - If you start as **paid** (users pay once at install), other rules apply (refunds, price changes, country pricing). Always read the current **App pricing** and **Paid apps** help in Play Console.

   **If you want both a free and a paid “version”**  
   Do **not** rely on flipping one listing from free to paid later. See **Part 2** for the two supported approaches (two Play apps vs one free app + billing).

4. **Merchant account (only if you charge money through Play)**  
   - **Free app, no in-app products:** often no payment profile needed at first.  
   - **Paid download** or **in-app purchases / subscriptions:** you typically complete **payments profile** and tax / merchant setup in Console. Google’s wizards change over time; follow what Play asks when you enable monetization.

5. **Privacy policy URL (often required)**  
   If your app collects personal data, uses sensitive permissions, handles payments, or Google’s questionnaire says you need one, you must host a **public privacy policy** page and paste its URL in Play Console. Plan a simple page (your website, GitHub Pages, etc.) before you finish the store listing.

---

## Part 2 — Free and paid plan (Option A selected)

For this project, use **Option A**: ship two separate Google Play apps.

- **Free app listing** (free download)
- **Paid app listing** (one-time paid download)

You cannot offer both free and paid download pricing from one package name, so this approach uses **two package IDs** and **two Play listings**.

| Item | Free app | Paid app |
|------|----------|----------|
| Play Console | **Create app** twice → two listings | Same |
| **Application ID** | e.g. `com.trolltrack.fishing` | **Must differ**, e.g. `com.trolltrack.fishing.pro` |
| Store default price | **Free** | **Paid** (set on App pricing before production if policy allows) |
| AAB | Build with free package ID | Build with pro package ID (different `ApplicationId`) |
| Version codes | Independent per app (each listing has its own sequence) | Independent |

**Steps (high level)**

1. **Decide package names** before you ship either build (they are permanent). Example: `com.trolltrack.fishing` and `com.trolltrack.fishing.pro`.
2. **Create the free app** in Play Console → choose **Free** on pricing during setup → complete testing (internal/closed) under that listing.
3. **Create the second app** for the paid SKU → choose **Paid** (or set price on **App pricing**) following Console prompts → use the **second** package name in your MAUI `ApplicationId` for that flavor.
4. **Engineering:** maintain either:
   - two build configurations (e.g. MSBuild `ApplicationId` / define constants per configuration), or  
   - a shared codebase with a thin “head” project per flavor,  
   so each AAB has the correct package name and any `#if` differences (feature flags).
5. **Signing:** you can use one keystore with two aliases or two keystores; register each app in Play App Signing separately.
6. **Google Maps (and other API keys):** in Google Cloud Console, add **both** package names and the correct **SHA-1/256** fingerprints for each build (upload and/or Play signing cert as applicable).
7. **Store listings:** different titles (“TrollTrack” vs “TrollTrack Pro”), descriptions, and optionally screenshots; cross-link in text (“Upgrade to Pro: …”).
8. **Invite-only testing:** each listing has its own **Internal testing** / **Closed testing** tracks and **opt-in links**. Add testers to **both** if you want them to try both apps.

**Important:** The **free** listing can never become the **paid** listing after you publish it as free (per Google’s rule above). The **paid** product is the **second** app.

---

## Part 3 — Understand your app’s Play identity

From this repo’s project file:

- **Application ID (package name):** `com.trolltrack.fishing`  
  This must **never** change **for that Play listing** after you publish; it is that app’s permanent ID on Play. Your paid app uses a second package ID (for example `com.trolltrack.fishing.pro`).
- **Display name:** TrollTrack  
- **Versioning:**  
  - `ApplicationDisplayVersion` — user-visible version (e.g. `1.0`).  
  - `ApplicationVersion` — **version code** (integer; must **increase** with every upload **to that app** on Play).

Every new file you upload must have a **higher version code** than the last one **for that same app**.

### 3.1 Step-by-step: update version numbers before each upload

You must increment the version code before every Play Console upload. Optionally update the display version when shipping a user-visible change.

1. Open `TrollTrack\TrollTrack.csproj`.
2. Find the **Versions** section near the top of the file:

```xml
<!-- Versions -->
<ApplicationDisplayVersion>1.0</ApplicationDisplayVersion>
<ApplicationVersion>2</ApplicationVersion>
```

3. **`ApplicationVersion`** (version code) — **Required** change for every upload.  
   - This is an **integer** that Google Play uses to determine if a build is newer.  
   - Increment by 1 each time (e.g. `2` → `3` → `4`).  
   - If you forget, Play Console will reject the upload with: *"Version code X has already been used."*

4. **`ApplicationDisplayVersion`** (display version) — **Optional** but recommended for user-facing releases.  
   - This is the version string users see in the Play Store listing (e.g. `1.0`, `1.1`, `2.0`).  
   - Update when shipping a meaningful change so testers/users can tell which version they have.  
   - Does **not** need to change for every upload (Play only checks the version code), but keeping it in sync avoids confusion.

5. Save the file.
6. **Rebuild the AAB** after saving — the version numbers are baked into the bundle at build time.

**Example progression:**

| Upload | `ApplicationVersion` | `ApplicationDisplayVersion` | Notes |
|--------|----------------------|-----------------------------|-------|
| First upload | `1` | `0.1.0` | Initial closed testing release |
| Bug fix | `2` | `0.1.0` | Same display version, code-only fix |
| New feature | `3` | `0.2.0` | Bumped display version for visible change |
| Another fix | `4` | `0.2.0` | Patch, no user-visible version change |

**Important:** The version code must **only go up** — you can never reuse or decrease it for a given Play app listing.

---

## Part 4 — Release build: Android App Bundle (AAB) and signing

Google Play **requires** **Android App Bundle (.aab)** for new apps (not a raw APK as the primary upload format).

### 4.1 Android packaging: APK for device deploy, AAB for Play Store

The project defaults to **APK** packaging for Android (`AndroidPackageFormat` / `AndroidPackageFormats` at the top of `TrollTrack.csproj`). That is intentional:

- **Visual Studio Deploy** with **AAB** as the primary format uses the Android SDK’s *bundle* install path, which runs **`adb uninstall`** before installing split APKs from the bundle. That **deletes your app’s private data** (including the SQLite database) on every deploy.
- **APK** deploy uses **`adb install -r`**, which upgrades the app **in place** and **keeps** trips, catches, and settings.

For **Google Play**, you still upload an **AAB**. Do **not** change the `.csproj` back to default `aab` for Release; pass the format only when publishing (see **4.6.4**).

Because this guide uses **Option A** (free + paid as separate apps), you will produce **two Release AABs** (one per package ID).

### 4.2 Step-by-step: use your existing keystore

You already have a keystore at:

- `C:\GitHub\TrollTrack\android-signing\release.keystore`

Use that file as your Android upload key.

1. Confirm the file exists at `C:\GitHub\TrollTrack\android-signing\release.keystore`.
2. Confirm you know:
   - Keystore password
   - Key alias
   - Key password
3. Keep `android-signing` out of git (you already do this, which is correct).
4. Back up `release.keystore` to at least one additional secure location.
5. Store passwords in a password manager. Never commit them to source control.

**Critical:** Losing this upload key can block or significantly delay future updates.

### 4.3 Step-by-step: configure Release signing in .NET MAUI

1. In `TrollTrack.csproj` (or a private, non-committed props file), configure Android Release signing to read from environment variables:
   - `AndroidKeyStore=true`
   - `AndroidSigningKeyStore` = `C:\GitHub\TrollTrack\android-signing\release.keystore`
   - `AndroidSigningKeyAlias` = `$(TROLLTRACK_ANDROID_KEY_ALIAS)`
   - `AndroidSigningKeyPass` = `$(TROLLTRACK_ANDROID_KEY_PASS)`
   - `AndroidSigningStorePass` = `$(TROLLTRACK_ANDROID_STORE_PASS)`
2. Create the environment variables on Windows (persistent, user-level):
   - Open **Start** and search for **Edit the system environment variables**.
   - Click **Environment Variables...**
   - Under **User variables**, click **New...** and add:
     - Variable name: `TROLLTRACK_ANDROID_KEY_ALIAS` / Value: your key alias
     - Variable name: `TROLLTRACK_ANDROID_KEY_PASS` / Value: your key password
     - Variable name: `TROLLTRACK_ANDROID_STORE_PASS` / Value: your keystore password
   - Click **OK** to save each variable, then close dialogs with **OK**.
3. Close and reopen your terminal and IDE so they pick up new environment variables.
4. Verify variables are available in a new terminal session:

```text
echo %TROLLTRACK_ANDROID_KEY_ALIAS%
echo %TROLLTRACK_ANDROID_KEY_PASS%
echo %TROLLTRACK_ANDROID_STORE_PASS%
```

5. If you prefer command line setup instead of UI, run once in Command Prompt:

```text
setx TROLLTRACK_ANDROID_KEY_ALIAS "your_alias_here"
setx TROLLTRACK_ANDROID_KEY_PASS "your_key_password_here"
setx TROLLTRACK_ANDROID_STORE_PASS "your_keystore_password_here"
```

6. If you only want temporary values for the current terminal session (not persisted), use:

```text
set TROLLTRACK_ANDROID_KEY_ALIAS=your_alias_here
set TROLLTRACK_ANDROID_KEY_PASS=your_key_password_here
set TROLLTRACK_ANDROID_STORE_PASS=your_keystore_password_here
```

7. Verify the keystore path is valid on your build machine.
8. If you use CI later, configure these same values in CI secrets and do not hard-code passwords in the repo.

### 4.4 Step-by-step: enroll in Play App Signing

1. In Play Console, open your app.
2. Go to the app signing area when prompted during first upload flow.
3. Choose Play App Signing (recommended/default for new apps).
4. Upload your first signed AAB.
5. Confirm the upload certificate fingerprint shown by Play matches your upload key.
6. Download/save Play’s app signing certificate details for future integrations (Maps/API restrictions, backend allowlists, etc.).

### 4.5 Step-by-step: configure Google Maps API key for release

Your build injects `GOOGLE_MAPS_API_KEY` into Android manifest during build.

1. In Google Cloud Console, create or reuse an Android-restricted API key for Maps SDK.
2. Add app restriction entries for the package/signing pairs you ship:
   - Free app package: `com.trolltrack.fishing` + correct SHA fingerprint(s)
   - Paid app package: your paid package ID (for example `com.trolltrack.fishing.pro`) + correct SHA fingerprint(s)
3. Start with upload key SHA fingerprint for local signed uploads.
4. After Play App Signing is active, add Play signing certificate SHA fingerprint as required by Google for installed-store builds.
5. **Grey map / no tiles on Play-installed builds** (while sideload or debug still works): store installs are signed with **Play App Signing**. Your key’s Android restriction must include package `com.trolltrack.fishing` and the **App signing certificate** SHA-1 from Play Console → **Setup** → **App integrity** (upload-key-only restrictions are a common miss). The Cloud project must also have **billing enabled** and **Maps SDK for Android** enabled for that key.
6. In your build terminal, set the key before publish:

```text
set GOOGLE_MAPS_API_KEY=your_release_key_here
```

### 4.6 Step-by-step: build the AAB

Follow these steps each time you need a signed **Android App Bundle** for upload to Play Console.

#### 4.6.1 Prerequisites

1. **.NET 9 SDK** installed, with the **.NET MAUI** workload (`dotnet workload install maui` if needed).
2. **Android SDK** available so `net9.0-android` builds succeed (Visual Studio Android workload or command-line setup).
3. Keystore present at `android-signing\release.keystore` (repo root), as described in **4.2**.
4. Environment variables for signing configured (**4.3**), or set in the **same** terminal session before you publish (**4.6.2**).

#### 4.6.2 Set environment variables (same terminal you build in)

If you did not set persistent user variables, set them for this session:

```text
set TROLLTRACK_ANDROID_KEY_ALIAS=your_key_alias
set TROLLTRACK_ANDROID_KEY_PASS=your_key_password
set TROLLTRACK_ANDROID_STORE_PASS=your_keystore_password
set GOOGLE_MAPS_API_KEY=your_maps_api_key
```

(If you already set `TROLLTRACK_ANDROID_*` and `GOOGLE_MAPS_API_KEY` as user variables, you can skip this step after opening a new terminal so they are loaded.)

#### 4.6.3 Open a terminal at the repository root

```text
cd C:\GitHub\TrollTrack
```

(Use your actual clone path if different.)

#### 4.6.4 Publish Release (produces the AAB)

Signing is configured in `TrollTrack.csproj` for `Release|net9.0-android`. Request **AAB** explicitly on the command line so Play gets a bundle while day-to-day VS deploy stays on APK (and preserves data):

```text
dotnet publish TrollTrack\TrollTrack.csproj -f net9.0-android -c Release -p:AndroidPackageFormat=aab
```

#### 4.6.5 Find the output file

Open this folder:

`TrollTrack\TrollTrack\bin\Release\net9.0-android\publish\`

You may see **two** `.aab` files for the same build, for example:

- One whose name includes **`-Signed`** (casing can vary, e.g. `-signed` or `-Signed`)
- One **without** that suffix

**Upload the signed bundle to Google Play:** use the file with **`-Signed`** / **`-signed`** in the name. That is the AAB signed with your release keystore. The other file is typically an **unsigned** (or pre-signing) artifact from the build pipeline; Play Console expects the **signed** AAB for upload.

If you only see one `.aab`, use that one (your tooling may collapse to a single output in some configurations).

#### 4.6.6 Free app vs paid app (Option A)

- **Free listing** (`com.trolltrack.fishing`): use the command above with the project’s default `ApplicationId`.
- **Paid listing**: build again only after your project uses the **paid** package ID for that variant (separate configuration or build property). The publish command is the same pattern; the artifact must match the correct Play app.

Rename or copy the `.aab` after each build if you build both (for example `TrollTrack-free-1.0.aab` vs `TrollTrack-pro-1.0.aab`) so you do not upload the wrong file to the wrong listing.

#### 4.6.7 If the build fails

1. **Signing errors** — Confirm `TROLLTRACK_ANDROID_KEY_ALIAS`, `TROLLTRACK_ANDROID_KEY_PASS`, and `TROLLTRACK_ANDROID_STORE_PASS` are set in the **same** session as `dotnet publish` (or restart the terminal after `setx`).
2. **Maps / manifest warnings** — Set `GOOGLE_MAPS_API_KEY` before publish; in Google Cloud, restrict the key to your package name and signing certificate SHA fingerprints (**4.5**).
3. **Android SDK errors** — Install/update the Android SDK and required API levels for your target framework.

#### 4.6.8 App crashes or closes immediately after installing from Play

If **Debug** builds work but the **Play Store** update fails on launch, common causes are:

1. **Wrong AAB** — Upload only the **`*-Signed` / `*-signed`** `.aab` from `publish\` (see **4.6.5**). An unsigned or mismatched artifact can behave badly after Play processing.
2. **Maps API key missing in the shipped manifest** — The release build must inject your real key. The project uses **`AndroidManifestPlaceholders`** so `dotnet publish` merges `MAPS_API_KEY` into the final manifest. Confirm `GOOGLE_MAPS_API_KEY` was set in the **same** terminal session as `dotnet publish`, and in Google Cloud restrict the key to package **`com.trolltrack.fishing`** and the **Play App Signing** certificate SHA-1 (and your upload key if you use key restrictions).
3. **Release linking** — `Release` uses `AndroidLinkMode` **SdkOnly** with a linker preserve file. If you still see native/.NET crashes only in Release, capture a trace: connect the device via USB, run `adb logcat`, reproduce the crash, and look for `AndroidRuntime`, `DEBUG`, or `mono`. As a **diagnostic** (not for production), you can temporarily set `AndroidLinkMode` to `None` in `TrollTrack.csproj` for `Release`, rebuild the AAB, and see if the crash disappears (that points at linking).
4. **Smoke-test the AAB locally** — Before upload, install a release build on a device (e.g. `adb install -r` on an APK produced from the same commit, or `bundletool` to install from the bundle). Catching startup failures before Play saves iteration time.

### 4.7 Pre-upload checklist (do this every release)

- Correct package ID for the app you are building (free vs paid)
- `ApplicationVersion` incremented for that specific listing
- Signing enabled and keystore path valid
- `GOOGLE_MAPS_API_KEY` set in current terminal session
- Output is `.aab` (not only `.apk`)
- Quick smoke test on a physical Android device before upload

---

## Part 5 — Create the app in Google Play Console

1. Open [Play Console](https://play.google.com/console).
2. **Create app**
   - App name: **TrollTrack** (or the store title you want).
   - Default language, app or game.
   - **Pricing:** create one app as **Free** and the second app as **Paid** (per Part 2).
   - Accept declarations (US export laws, content policies, etc.).

3. **Dashboard** will show **setup tasks**. Work through them in order. Typical sections:

### 5.1 App access

If parts of the app require login and reviewers cannot access it, you must provide **test credentials** or a **demo** path. For invite-only testing, you still complete this if Google’s review applies to your track.

### 5.2 Ads

State whether the app contains ads.

### 5.3 Content rating questionnaire

Complete the **IARC** (or regional) questionnaire honestly. You receive a rating; without it you cannot publish.

### 5.4 Target audience and content

Declare whether the app is directed at children (COPPA / Families policies). Fishing logs are usually **not** child-directed unless you specifically target kids.

### 5.5 News app, COVID-19, etc.

Answer **No** if not applicable.

### 5.6 Data safety

Declare what data you collect, whether it is encrypted in transit, whether users can request deletion, etc. This must match your app behavior and privacy policy. If you use **billing**, answer purchase-related questions accurately.

### 5.7 Government apps, financial features, etc.

Answer as appropriate for TrollTrack.

### 5.8 Store listing (minimum for a testing release)

- **Short description**, **full description**
- **App icon** (512×512 PNG, compliance with Play icon rules)
- **Feature graphic** (1024×500) — required for some surfaces
- **Screenshots** (phone; tablet if you claim tablet support)

You can refine these before promoting to production.

---

## Part 6 — Invite-only distribution: Internal testing (recommended first)

### 6.1 Create an internal testing release

1. In Play Console, open your app → **Testing** → **Internal testing**.
2. **Create a new release**.
3. Upload your **`.aab`** file.
4. Add **release notes** (what testers should know).

### 6.2 Testers

1. In **Internal testing** → **Testers** tab, create an **email list** or use **Google Groups**.
2. Add tester Gmail addresses (each tester needs a Google account).

### 6.3 Opt-in link

Play provides an **opt-in URL**. Send that link only to invited testers. After they accept:

- They open the **Play Store** and install **TrollTrack** like any other app (updates come through Play).

### 6.4 Review timeline

- **Internal testing** is often the **fastest** path; Google may still run checks. First-time apps sometimes wait hours to a few days.

### 6.5 Version updates

For each fix:

1. Bump **`ApplicationVersion`** (version code) in the project **for that app’s package**.
2. Optionally bump **`ApplicationDisplayVersion`**.
3. Build a new **Release AAB**, upload a **new release** in the same internal track, roll out.

---

## Part 7 — Closed testing (more testers, still invite-only)

If you outgrow 100 testers or want a separate “beta” group:

1. **Testing** → **Closed testing** → create a **closed track** (you can name it e.g. “Beta”).
2. Upload **AAB** (same signing; new version code if it is a different build).
3. Add testers via **email list** or **Google Group**.
4. Share the **closed testing opt-in link**.

Rules and review may differ slightly from internal; both keep the app **out of production search** until you promote to production.

---

## Part 8 — What is *not* public yet

While you only release on **internal** or **closed** testing:

- The app does **not** appear in general Play Store search for the public.
- Only people on your tester list who **opt in** can install via the testing link (subject to Google’s current UI).

When you are ready for everyone: create a **Production** release and go through **pre-launch report** and any country rollout options.

---

## Part 9 — Checklist before each upload

- [ ] `ApplicationVersion` (version code) **incremented** for **this** Play app
- [ ] **Release** configuration; **`dotnet publish` with `-p:AndroidPackageFormat=aab`** for the upload artifact
- [ ] Correct **`ApplicationId`** for free vs paid app package
- [ ] Signed with your **upload** keystore (passwords available)
- [ ] `GOOGLE_MAPS_API_KEY` set for release; Maps restrictions match signing certs **and** package name(s)
- [ ] Smoke-test the Release build on a physical device
- [ ] Play Console **Data safety** and **store listing** still accurate
- [ ] **App pricing** is set correctly on each listing before first production publish (free listing stays free; paid listing is paid)

---

## Part 10 — Common first-time issues

1. **Wrong file type** — Play rejects APK when AAB is required; ensure `dotnet publish` produces **.aab** for the upload you use.  
2. **Version code reused** — Each upload must have a **strictly higher** integer than any previous upload to **that** app.  
3. **Signing mismatch** — Upload key must stay consistent; if you enroll in Play App Signing, follow Google’s certificate screens.  
4. **Maps blank in release** — API key restrictions or SHA fingerprint do not match the signing certificate used for the build users install.  
5. **Privacy policy** — Missing or generic policy when the questionnaire implies you need a detailed one can block release.  
6. **Expected to turn a free listing into paid later** — Not allowed after publish; your paid offering must be a **second app listing**.

---

## Part 11 - Common PowerShell commands

1. Push database to emulator
   - adb -s emulator-5554 push "C:\GitHub\TrollTrack\trolltrack_testing.db" /data/local/tmp/trolltrack_backup.db
   - adb -s emulator-5554 shell run-as com.trolltrack.fishing cp /data/local/tmp/trolltrack_backup.db /data/data/com.trolltrack.fishing/files/trolltrack.db


## Part 12 — Official references (bookmark these)

- [Google Play Console](https://play.google.com/console)  
- [Set up an open, closed, or internal test](https://support.google.com/googleplay/android-developer/answer/9845334)  
- [Android App Bundle](https://developer.android.com/guide/app-bundle)  
- [.NET MAUI Android deployment](https://learn.microsoft.com/en-us/dotnet/maui/android/deployment)  
- [Play App Signing](https://support.google.com/googleplay/android-developer/answer/9842756)  
- Search Play Console Help for **App pricing** and **Paid apps** for the latest on free vs paid download rules.

---

## Summary

1. Register a **Play developer account** and pay the fee.  
2. Use **two Play apps** for free and paid versions: one package name per listing (Part 2).  
3. Configure **Release AAB** builds and **signing** (upload keystore + Play App Signing).  
4. Fix **Maps** key for release signing (all package names you ship).  
5. Create the app(s) in **Play Console** and complete **policy, data safety, content rating, store listing**.  
6. Upload the **AAB** to **Internal testing**, add **tester emails**, share the **opt-in link**.  
7. Iterate with **higher version codes** on each upload.  
8. Move to **Production** only when you want a public launch.

This document is a roadmap; exact Play Console menus change occasionally—use Google’s support pages for screenshots and wording if something moved.
