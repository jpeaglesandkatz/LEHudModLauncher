using MaterialSkin;
using MaterialSkin.Controls;

namespace LEHuDModLauncher.Classlibs;

public partial class ThemePickerForm : MaterialForm
{
    private readonly MaterialSkinManager _skinManager;

    public ThemePickerForm()
    {
        InitializeComponent();

        // MaterialSkin setup
        _skinManager = MaterialSkinManager.Instance;
        _skinManager.EnforceBackcolorOnAllComponents = true;
        _skinManager.AddFormToManage(this);

        // Populate dropdowns with enum values
        cmbPrimary.DataSource = Enum.GetValues<Primary>();
        cmbAccent.DataSource = Enum.GetValues<Accent>();
        cmbTheme.DataSource = Enum.GetValues<MaterialSkinManager.Themes>();
    }

    private void btnApply_Click(object sender, EventArgs e)
    {
        var primary = (Primary)cmbPrimary.SelectedItem;
        var accent = (Accent)cmbAccent.SelectedItem;
        var theme = (MaterialSkinManager.Themes)cmbTheme.SelectedItem;

        // Apply live
        _skinManager.Theme = theme;
        _skinManager.ColorScheme = new ColorScheme(
            primary,
            primary,
            primary,
            accent,
            TextShade.WHITE
        );

        // Save to JSON
        ThemeManagerHelper.SaveTheme(primary, accent, theme);

        MessageBox.Show("Theme applied and saved!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

}