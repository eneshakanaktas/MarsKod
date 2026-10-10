using System;
using UnityEngine;

// Kanyon deposu (Bolum 53-57): alanin arkasinda alcak, uzun, acik bir sundurma. Ince direkler, arkaya egik oluklu sac cati;
// firtina catinin ortasini sokmus: rafin ustu acik (yukaridan bakan kamera raf ve Serce'yi gorur), solda ve sagda sac
// levhalar kalmis. Bir ucta on kirisden sarkan tek branda. Alani saran duvar yok; en yuksek noktasi ufku kesmeyecek kadar
// alcak. 55'te firtinanin izi daha buyuk: sag taraftan iki levha daha yok, arka kiris kirik sarkiyor.
public static class DepotShed
{
    public readonly struct Spots
    {
        public readonly Vector3 ShelfTop;    // rafin ust gozunun ortasi (Serce burada durur)
        public readonly Vector3 PostFace;    // rafin sagindaki ilk on diregin kameraya bakan yuzu (pano buraya asilir)
        public readonly Transform TarpFace;  // brandanin kameraya bakan yuzu (boya ok buraya)
        public Spots(Vector3 shelfTop, Vector3 postFace, Transform tarpFace) { ShelfTop = shelfTop; PostFace = postFace; TarpFace = tarpFace; }
    }

    // alcak: cati, kamera alana yaklastiginda da (kisa kodlu bolumler) uzaktaki kayaliklarin ustune binmez
    const float FrontH = 0.68f, BackH = 0.58f, Depth = 0.85f, PostSpacing = 1.2f, Post = 0.06f, SheetW = 0.8f;
    const float ShelfTopY = 0.4f;                   // sundurmanin altindaki rafin ust gozu (catidan alcak)
    const float BrokenDrop = 0.7f;                  // 55'te kopan kirisin sarkan ucunun dususu
    const float RoofGapMargin = 0.25f;              // rafin iki yaninda catinin acik kalan payi

    // Malzemeler bir kez uretilir, her bolumde paylasilir
    static readonly Material PostMat = Mats.Lit(Mats.Hex("#3A3230"), 0.35f);
    static readonly Material BeamMat = Mats.Lit(Mats.Hex("#4A403C"), 0.35f);
    static readonly Material SheetMat = Mats.Lit(Mats.Hex("#8A7F76"), 0.4f);
    static readonly Material RibMat = Mats.Lit(Mats.Hex("#6A6058"), 0.4f);
    static readonly Material TarpMat = Mats.Lit(Mats.Hex("#C9B8A0"), 0.1f);

    // frontCenter: on direk sirasinin ortasi (x, z; y kullanilmaz). width: boyu. bigHole: 55'teki cati deligi.
    // ground: zemin yuksekligi; her direk kendi yerine oturur.
    public static Spots Build(Transform parent, Vector3 frontCenter, float width, bool bigHole, Func<float, float, float> ground)
    {
        var root = Parts.Empty("Iz-Sundurma", parent);
        float x0 = frontCenter.x, zFront = frontCenter.z, zBack = zFront + Depth;
        float baseY = ground(x0, zFront + Depth * 0.5f);

        Posts(root, x0, zFront, zBack, width, ground);
        Beams(root, x0, zFront, zBack, width, baseY, bigHole);

        // raf ortadan biraz solda (sagda pusula ve dugmeler var); uzun sundurmada iki, kisada bir kare boyu
        float shelfLength = width >= 5f ? 2f : 1f, shelfX = x0 - shelfLength * 0.25f;
        var shelfPos = new Vector3(shelfX, ground(shelfX, zBack - 0.45f), zBack - 0.45f);
        var shelf = StorageShelf.Create(root, shelfPos, shelfLength, stocked: true, ShelfTopY);
        float gapHalf = shelfLength * 0.5f + RoofGapMargin;
        Roof(root, x0, zFront, width, baseY, shelfX - gapHalf, shelfX + gapHalf, bigHole);
        var tarpFace = Tarp(root, new Vector3(x0 - width * 0.5f + 0.55f, baseY + FrontH - 0.03f, zFront - 0.02f));

        float boardX = FirstPostRightOf(x0, width, shelfX + gapHalf + 0.35f);
        var postFace = new Vector3(boardX, ground(boardX, zFront) + FrontH * 0.55f, zFront - Post * 0.5f - 0.005f);
        return new Spots(shelf.localPosition + StorageShelf.TopCenter(ShelfTopY), postFace, tarpFace);
    }

    // On sira ile arka sira direkleri; her biri zemine oturur
    static void Posts(Transform root, float x0, float zFront, float zBack, float width, Func<float, float, float> ground)
    {
        int count = PostCount(width);
        for (int i = 0; i < count; i++)
        {
            float x = PostX(x0, width, i, count);
            AddPost(root, x, zFront, FrontH, ground);
            AddPost(root, x, zBack, BackH, ground);
        }
    }

    static void AddPost(Transform root, float x, float z, float height, Func<float, float, float> ground)
    {
        float y = ground(x, z);
        Parts.Add("Direk", root, MeshFactory.RoundedBox(new Vector3(Post, height, Post), 0.01f), PostMat, new Vector3(x, y + height * 0.5f, z));
    }

    static int PostCount(float width) => Mathf.Max(2, Mathf.RoundToInt(width / PostSpacing) + 1);
    static float PostX(float x0, float width, int i, int count) => x0 - width * 0.5f + i * width / (count - 1);

    // minX'in sagindaki ilk direk (yoksa en sagdaki): pano ekranin kenarina degil, rafin yanina asilir
    static float FirstPostRightOf(float x0, float width, float minX)
    {
        int count = PostCount(width);
        for (int i = 0; i < count; i++)
        {
            float x = PostX(x0, width, i, count);
            if (x >= minX) return x;
        }
        return x0 + width * 0.5f;
    }

    // On ve arka kiris (direklerin tepesi) ile iki uctaki yan kiris. bigHole: arka kiris ortadan kirik, bir yarisi sarkik.
    static void Beams(Transform root, float x0, float zFront, float zBack, float width, float baseY, bool bigHole)
    {
        var size = new Vector3(width + 0.1f, 0.07f, 0.08f);
        Parts.Add("Kiris", root, MeshFactory.RoundedBox(size, 0.015f), BeamMat, new Vector3(x0, baseY + FrontH, zFront));
        if (!bigHole)
            Parts.Add("Kiris", root, MeshFactory.RoundedBox(size, 0.015f), BeamMat, new Vector3(x0, baseY + BackH, zBack));
        else
        {
            float halfLength = width * 0.5f;
            var half = new Vector3(halfLength, 0.07f, 0.08f);
            Parts.Add("Kiris", root, MeshFactory.RoundedBox(half, 0.015f), BeamMat, new Vector3(x0 - width * 0.25f, baseY + BackH, zBack));
            // sag yari kopmus: sag ucu direkte, sol ucu BrokenDrop kadar asagi sarkmis
            float angle = Mathf.Asin(BrokenDrop / halfLength);
            var broken = Parts.Add("KirikKiris", root, MeshFactory.RoundedBox(half, 0.015f), BeamMat,
                new Vector3(x0 + width * 0.5f - halfLength * 0.5f * Mathf.Cos(angle), baseY + BackH - BrokenDrop * 0.5f, zBack));
            broken.localRotation = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg);
        }
        foreach (float sx in new[] { -1f, 1f })
        {
            var side = Parts.Add("Kiris", root, MeshFactory.RoundedBox(new Vector3(0.07f, 0.07f, Depth + 0.1f), 0.015f), BeamMat,
                new Vector3(x0 + sx * width * 0.5f, baseY + (FrontH + BackH) * 0.5f, (zFront + zBack) * 0.5f));
            side.localRotation = Quaternion.Euler(RoofTilt, 0f, 0f);
        }
    }

    static float RoofTilt => Mathf.Atan2(FrontH - BackH, Depth) * Mathf.Rad2Deg;   // arka uc asagi

    // Cati: rafin ustundeki acikligin iki yaninda sac levhalar. gapLeft/gapRight: acikligin kenarlari (x).
    // bigHole: sag taraftaki acikliga en yakin iki levha da yok.
    static void Roof(Transform root, float x0, float zFront, float width, float baseY, float gapLeft, float gapRight, bool bigHole)
    {
        float y = baseY + (FrontH + BackH) * 0.5f + 0.05f, z = zFront + Depth * 0.5f;
        SheetRow(root, x0 - width * 0.5f, gapLeft, y, z, skipNearGap: 0, tornSide: 1f);
        SheetRow(root, x0 + width * 0.5f, gapRight, y, z, skipNearGap: bigHole ? 2 : 0, tornSide: -1f);
    }

    // Bir yandaki levhalar: dis uctan (outerX) acikliga (gapX) dogru dizilir. Acikliga en yakin levha yirtik: daha kisa ve
    // on ucu asagi kivrilmis. skipNearGap: acikliga en yakin kac levha yok.
    static void SheetRow(Transform root, float outerX, float gapX, float y, float z, int skipNearGap, float tornSide)
    {
        float span = Mathf.Abs(gapX - outerX), dir = Mathf.Sign(gapX - outerX);
        int count = Mathf.Max(1, Mathf.RoundToInt(span / SheetW));
        float w = span / count;
        for (int i = 0; i < count - skipNearGap; i++)
        {
            bool torn = i == count - 1 - skipNearGap;
            float x = outerX + dir * (i + 0.5f) * w;
            float depth = torn ? Depth * 0.7f : Depth + 0.15f;
            var sheet = Parts.Empty("CatiLevhasi", root);
            sheet.localPosition = new Vector3(x, y, z - (Depth + 0.15f - depth) * 0.5f);
            sheet.localRotation = Quaternion.Euler(RoofTilt - (torn ? 12f : 0f), 0f, (torn ? 5f * tornSide : 0f) + 1.2f * Mathf.Sin(i * 1.9f));
            Parts.Add("Sac", sheet, MeshFactory.RoundedBox(new Vector3(w - 0.04f, 0.02f, depth), 0.008f), SheetMat, Vector3.zero);
            for (int r = 0; r < 3; r++)   // oluklar: levha boyunca uc ince cizgi
                Parts.Add("Oluk", sheet, MeshFactory.RoundedBox(new Vector3(0.025f, 0.012f, depth - 0.04f), 0.005f), RibMat,
                    new Vector3((r - 1) * w * 0.3f, 0.014f, 0f), outline: false, castShadow: false);
        }
    }

    // On kirisden sarkan tek branda; hafif egik. Doner: kameraya bakan yuzu (boya ok icin).
    static Transform Tarp(Transform root, Vector3 hangPoint)
    {
        const float w = 0.75f, h = 0.5f;
        var tarp = Parts.Empty("Branda", root);
        tarp.localPosition = hangPoint;
        tarp.localRotation = Quaternion.Euler(-6f, 0f, 4f);
        Parts.Add("Bez", tarp, MeshFactory.RoundedBox(new Vector3(w, h, 0.015f), 0.006f), TarpMat, new Vector3(0f, -h * 0.5f, 0f), outline: false);
        var face = Parts.Empty("Yuzey", tarp);
        face.localPosition = new Vector3(0f, -h * 0.5f, -0.012f);
        return face;
    }
}
